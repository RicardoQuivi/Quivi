using Quivi.Application.Commands.Sessions;
using Quivi.Application.Commands.Users;
using Quivi.Application.Extensions;
using Quivi.Application.Extensions.Pos;
using Quivi.Application.Pos.Items;
using Quivi.Application.Queries.Channels;
using Quivi.Application.Queries.MerchantAcquirerConfigurations;
using Quivi.Application.Queries.Merchants;
using Quivi.Application.Queries.Orders;
using Quivi.Application.Queries.People;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Entities.Identity;
using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Repositories;
using System.Globalization;

namespace Quivi.Application.Commands.PosCharges
{
    public class CreateGuestPosChargeAsyncCommand : ICommand<Task<PosCharge?>>
    {
        public class PayAtTheTable
        {
            public IEnumerable<SessionItem>? Items { get; init; }
        }

        public class OrderAndPay
        {
            public int OrderId { get; init; }
        }

        public int ChannelId { get; init; }
        public int MerchantAcquirerConfigurationId { get; init; }
        public int? ConsumerPersonId { get; init; }
        public string? VatNumber { get; init; }
        public string? Email { get; init; }
        public decimal Amount { get; init; }
        public decimal Tip { get; init; }
        public PayAtTheTable? PayAtTheTableData { get; init; }
        public OrderAndPay? OrderAndPayData { get; init; }


        public ChargeMethod? SurchargeFeeOverride { get; init; }
        public required string UserLanguageIso { get; init; }

        public required Action OnInvalidAdditionalData { get; init; }
        public required Action OnInvalidTip { get; init; }
        public required Action OnInvalidAmount { get; init; }
        public required Action OnInvalidChannel { get; init; }
        public required Action OnInvalidMerchantAcquirerConfiguration { get; init; }
        public required Action OnNoOpenSession { get; init; }
    }

    public class CreateGuestPosChargeAsyncCommandHandler : ICommandHandler<CreateGuestPosChargeAsyncCommand, Task<PosCharge?>>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IUnitOfWork unitOfWork;
        private readonly IPosChargesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly ICommandProcessor commandProcessor;
        private readonly IRandomGenerator randomGenerator;
        private readonly IEventService eventService;

        public CreateGuestPosChargeAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                        IUnitOfWork unitOfWork,
                                                        IDateTimeProvider dateTimeProvider,
                                                        ICommandProcessor commandProcessor,
                                                        IRandomGenerator randomGenerator,
                                                        IEventService eventService)
        {
            this.queryProcessor = queryProcessor;
            this.unitOfWork = unitOfWork;
            this.repository = unitOfWork.PosCharges;
            this.dateTimeProvider = dateTimeProvider;
            this.commandProcessor = commandProcessor;
            this.randomGenerator = randomGenerator;
            this.eventService = eventService;
        }

        private record TransactionInfo
        {
            public required Merchant Merchant { get; init; }
            public required Channel Channel { get; init; }
            public required MerchantAcquirerConfiguration MerchantAcquirerConfiguration { get; init; }
            public required Person Consumer { get; init; }
            public decimal Tip { get; init; }
            public decimal BaseAmount { get; init; }
            public decimal MerchantCaptureAmount => Tip + BaseAmount;
            public decimal SurchargeAmount { get; init; }
            public decimal AppliedFeeValue { get; init; }
            public FeeUnit AppliedFeeUnit { get; init; }
            public decimal TotalChargeAmount => Tip + BaseAmount + SurchargeAmount;
        }

        public async Task<PosCharge?> Handle(CreateGuestPosChargeAsyncCommand command)
        {
            if (command.Tip < 0)
            {
                command.OnInvalidTip();
                return null;
            }

            if (command.Amount < 0)
            {
                command.OnInvalidAmount();
                return null;
            }

            if (command.Tip + command.Amount <= 0)
            {
                command.OnInvalidAmount();
                command.OnInvalidTip();
                return null;
            }

            if (command.PayAtTheTableData != null && command.OrderAndPayData != null)
            {
                command.OnInvalidAdditionalData();
                return null;
            }

            var transactionInfo = await GetTransactionInfo(command);
            if (transactionInfo == null)
                return null;

            await using var transaction = await unitOfWork.StartTransactionAsync();
            var posCharge = await Process(transactionInfo, command);
            if (posCharge == null)
                return null;

            await StartAcquiring(transactionInfo, posCharge);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            await eventService.Publish(new OnPosChargeOperationEvent
            {
                Id = posCharge.ChargeId,
                MerchantId = posCharge.MerchantId,
                ChannelId = posCharge.ChannelId,
                Operation = EntityOperation.Create,
            });

            return posCharge;
        }

        private async Task<PosCharge?> Process(TransactionInfo transactionInfo, CreateGuestPosChargeAsyncCommand command)
        {
            int? sessionId = null;
            DateTime now = dateTimeProvider.GetUtcNow();

            var depositPersonQuery = await queryProcessor.Execute(new GetPeopleAsyncQuery
            {
                ChannelIds = [transactionInfo.Channel.Id],
                ClientTypes = [BasicAuthClientType.GuestsApp],
                PageSize = 1,
            });
            var person = depositPersonQuery.Single();

            var posCharge = new PosCharge
            {
                MerchantId = transactionInfo.Merchant.Id,
                ChannelId = transactionInfo.Channel.Id,
                Total = transactionInfo.TotalChargeAmount,
                Payment = transactionInfo.BaseAmount,
                Tip = transactionInfo.Tip,
                SurchargeFeeAmount = transactionInfo.SurchargeAmount,
                SessionId = sessionId,
                LocationId = null,
                Email = command.Email,
                VatNumber = command.VatNumber,
                Observations = null,
                CaptureDate = null,
                Charge = new Charge
                {
                    ChargePartner = transactionInfo.MerchantAcquirerConfiguration.ChargePartner,
                    ChargeMethod = transactionInfo.MerchantAcquirerConfiguration.ChargeMethod,

                    MerchantAcquirerConfigurationId = transactionInfo.MerchantAcquirerConfiguration.Id,
                    MerchantAcquirerConfiguration = transactionInfo.MerchantAcquirerConfiguration,

                    ChainedChargeId = null,
                    Status = ChargeStatus.Requested,
                    Deposit = new Deposit
                    {
                        Amount = ChargeMethod.Wallet == transactionInfo.MerchantAcquirerConfiguration.ChargeMethod ? 0.0M : transactionInfo.TotalChargeAmount,
                        ConsumerId = transactionInfo.Consumer.Id,
                        DepositCapture = new DepositCapture
                        {
                            Type = JournalType.Capture,
                            Amount = transactionInfo.MerchantCaptureAmount,
                            PersonId = person.Id,
                        },
                        DepositSurchage = transactionInfo.SurchargeAmount > 0 ? new DepositSurcharge
                        {
                            Amount = transactionInfo.SurchargeAmount,
                            AppliedUnit = transactionInfo.AppliedFeeUnit,
                            AppliedValue = transactionInfo.AppliedFeeValue,
                        } : null,
                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                    CreatedDate = now,
                    ModifiedDate = now,
                },
                CreatedDate = now,
                ModifiedDate = now,
            };

            if (await AddPaymentTypeInformation(command, posCharge, now) == false)
                return null;

            if (posCharge.Payment + posCharge.Tip + posCharge.SurchargeFeeAmount != posCharge.Total)
                throw new Exception("If this happens it should mean the amount the user is trying to pay is no longer valid");

            repository.Add(posCharge);
            return posCharge;
        }

        private static decimal CalculateItemAmount(SessionItem item) => item.Quantity == 0 ? 0 : item.Quantity * item.GetUnitPrice();

        private async Task<bool> AddPaymentTypeInformation(CreateGuestPosChargeAsyncCommand command, PosCharge posCharge, DateTime now)
        {
            if (command.OrderAndPayData == null && command.PayAtTheTableData == null) // Free payment, no check needed
                return true;

            if (command.PayAtTheTableData != null)
            {
                var sessionQuery = await queryProcessor.Execute(new GetSessionsAsyncQuery
                {
                    ChannelIds = [command.ChannelId],
                    Statuses = [SessionStatus.Ordering],
                    IncludeOrdersMenuItems = true,
                    IncludeOrdersMenuItemsPosChargeInvoiceItems = true,
                    PageSize = 1,
                });
                var session = sessionQuery.SingleOrDefault();
                if (session == null)
                {
                    command.OnNoOpenSession();
                    return false;
                }

                if (ValidatePayAtTheTablePayment(command, session) == false)
                    return false;

                posCharge.SessionId = session.Id;
                if (command.PayAtTheTableData.Items?.Any() != true)
                {
                    posCharge.Payment = command.Amount;
                    return true;
                }

                var totalPrice = command.PayAtTheTableData.Items.Sum(CalculateItemAmount);
                posCharge.Payment = totalPrice;

                var comparer = new SessionItemComparer();
                var allAvailableItems = session.Orders!.SelectMany(o => o.OrderMenuItems!.Select(i => new
                {
                    OrderMenuItem = i,
                    PaidQuantity = i.PosChargeInvoiceItems!.Sum(ii => ii.Quantity),
                    SessionItem = i.AsSessionItem(),
                })).Where(e => e.OrderMenuItem.Quantity > e.PaidQuantity).GroupBy(g => g.SessionItem, comparer)
                                                                            .ToDictionary(s => s.Key, s => s.AsEnumerable(), comparer);

                posCharge.PosChargeSelectedMenuItems = new List<PosChargeSelectedMenuItem>();
                foreach (var payingItem in command.PayAtTheTableData.Items)
                {
                    var quantity = payingItem.Quantity;
                    if (allAvailableItems.TryGetValue(payingItem, out var availableItems) == false)
                        return false;

                    foreach (var availableItem in availableItems)
                    {
                        var availableQuantity = availableItem.OrderMenuItem.Quantity - availableItem.PaidQuantity;
                        decimal quantityToTake = quantity - availableQuantity >= 0 ? availableQuantity : quantity;

                        quantity -= quantityToTake;
                        posCharge.PosChargeSelectedMenuItems.Add(new PosChargeSelectedMenuItem
                        {
                            OrderMenuItem = availableItem.OrderMenuItem,
                            OrderMenuItemId = availableItem.OrderMenuItem.Id,
                            Quantity = quantityToTake,
                            CreatedDate = now,
                            ModifiedDate = now,
                        });

                        if (quantity == 0)
                            break;
                    }

                    if (quantity != 0)
                        return false;
                }
                return true;
            }

            if (command.OrderAndPayData != null)
            {
                var orderQuery = await queryProcessor.Execute(new GetOrdersAsyncQuery
                {
                    Ids = [command.OrderAndPayData.OrderId],
                    ChannelIds = [command.ChannelId],
                    IncludeOrderMenuItems = true,
                    IncludeChannelProfile = true,
                    PageSize = 1,
                });

                var order = orderQuery.SingleOrDefault();
                if (order == null)
                {
                    command.OnInvalidAdditionalData();
                    return false;
                }

                if (ValidateOrderAndPay(command, order) == false)
                    return false;

                posCharge.PosChargeSelectedMenuItems = order.OrderMenuItems!.Select(om => new PosChargeSelectedMenuItem
                {
                    OrderMenuItem = om,
                    OrderMenuItemId = om.Id,
                    Quantity = om.Quantity,
                    CreatedDate = now,
                    ModifiedDate = now,
                }).ToList();
                return true;
            }
            return false;
        }

        private static bool ValidateOrderAndPay(CreateGuestPosChargeAsyncCommand command, Order order)
        {
            if (order.State != OrderState.Draft)
            {
                command.OnInvalidAdditionalData();
                return false;
            }

            var total = order.OrderMenuItems!.Sum(e => e.Quantity * e.FinalPrice);
            if (total != command.Amount)
            {
                command.OnInvalidAdditionalData();
                return false;
            }

            if (order.PayLater == false && (order.Channel!.ChannelProfile!.PrePaidOrderingMinimumAmount ?? 0.0m) > total)
            {
                command.OnInvalidChannel();
                return false;
            }

            return true;
        }

        private bool ValidatePayAtTheTablePayment(CreateGuestPosChargeAsyncCommand command, Session session)
        {
            var items = session.Orders!.SelectMany(o => o.OrderMenuItems!).AsConvertedSessionItems();
            var totals = items.Aggregate((total: 0.0M, totalPaid: 0.0M), (r, item) =>
            {
                var first = item.Source.First();
                var paidQuantity = item.Source.SelectMany(s => s.PosChargeInvoiceItems!).Sum(s => s.Quantity);

                r.total += item.Quantity * item.Price;
                r.totalPaid += paidQuantity * item.Price;
                return r;
            });

            var outstanding = totals.total - totals.totalPaid;

            var unmatchedAmount = command.Amount - outstanding;
            if (unmatchedAmount > 0)
            {
                var errorMessage = $"{GetType().Name}: Amount ({command.Amount}) is higher than outstanding ({outstanding}). Unmatched amount is {unmatchedAmount}.";
                var sessionItemCount = items.Sum(r => r.Quantity);

                //since quantity is rounded to two decimal cases,
                //there may be a max difference of 0.01 per item being paid
                var allowedUnmatchedAmount = 0.01m * sessionItemCount;
                if (unmatchedAmount > allowedUnmatchedAmount)
                {
                    command.OnInvalidAmount();
                    return false;
                }
            }

            return true;
        }

        private async Task<TransactionInfo?> GetTransactionInfo(CreateGuestPosChargeAsyncCommand command)
        {
            var channelId = command.ChannelId;
            var tip = command.Tip;
            var baseAmount = command.Amount;

            var merchantQuery = await queryProcessor.Execute(new GetMerchantsAsyncQuery
            {
                ChannelIds = [channelId],
                IncludeFees = true,
                IsDeleted = false,
                PageIndex = 0,
                PageSize = 1,
            });
            var merchant = merchantQuery.SingleOrDefault();
            if (merchant == null)
            {
                command.OnInvalidChannel();
                return null;
            }

            var channelQuery = await queryProcessor.Execute(new GetChannelsAsyncQuery
            {
                Ids = [channelId],
                IsDeleted = false,
                PageSize = 1,
            });
            var channel = channelQuery.SingleOrDefault();
            if (channel == null)
            {
                command.OnInvalidChannel();
                return null;
            }

            var merchantAcquirerConfigurationQuery = await queryProcessor.Execute(new GetMerchantAcquirerConfigurationsAsyncQuery
            {
                ChannelIds = [channelId],
                Ids = [command.MerchantAcquirerConfigurationId],
                IsDeleted = false,
                PageIndex = 0,
                PageSize = 1,
            });
            var acquirerConfiguration = merchantAcquirerConfigurationQuery.SingleOrDefault();
            if (acquirerConfiguration == null)
            {
                command.OnInvalidMerchantAcquirerConfiguration();
                return null;
            }

            decimal surchargeAmount = 0;
            decimal appliedFeeValue = 0;
            FeeUnit appliedFeeUnit = default;

            bool skipSurcharge = SkipSurcharge(command, merchant);
            if (!skipSurcharge)
            {
                var defaultSurchargeFee = merchant.SurchargeFee;
                var defaultSurchargeFeeUnit = merchant.SurchargeFeeUnit;
                var feeOverride = merchant.SurchargeFees?.SingleOrDefault(f => f.ChargeMethod == (command.SurchargeFeeOverride ?? acquirerConfiguration.ChargeMethod));

                var feeValue = feeOverride?.Fee ?? defaultSurchargeFee;
                var feeUnit = feeOverride?.FeeUnit ?? defaultSurchargeFeeUnit;

                surchargeAmount = Math.Round((tip + baseAmount).GetFee(feeValue, feeUnit), 2, MidpointRounding.AwayFromZero);
                appliedFeeValue = feeValue;
                appliedFeeUnit = feeUnit;
            }

            Person consumer = await GetOrCreateConsumer(command);
            return new TransactionInfo
            {
                Merchant = merchant,
                Channel = channel,
                MerchantAcquirerConfiguration = acquirerConfiguration,
                Consumer = consumer,
                Tip = tip,
                BaseAmount = baseAmount,
                SurchargeAmount = surchargeAmount,
                AppliedFeeValue = appliedFeeValue,
                AppliedFeeUnit = appliedFeeUnit,
            };
        }

        private bool SkipSurcharge(CreateGuestPosChargeAsyncCommand command, Merchant eatsMerchant)
        {
            bool ignorePortuguese = eatsMerchant.DisabledFeatures.HasFlag(MerchantFeature.AllowSurchargeFeeForPT);
            bool userIsPortuguese = CultureInfo.GetCultureInfo(command.UserLanguageIso).TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase);
            return ignorePortuguese && userIsPortuguese;
        }

        private async Task<Person> GetOrCreateConsumer(CreateGuestPosChargeAsyncCommand command)
        {
            if (command.ConsumerPersonId.HasValue)
            {
                var query = await queryProcessor.Execute(new GetPeopleAsyncQuery
                {
                    Ids = [command.ConsumerPersonId.Value],
                    PageSize = 1,
                });
                return query.Single();
            }

            if (string.IsNullOrWhiteSpace(command.Email))
            {
                var query = await queryProcessor.Execute(new GetPeopleAsyncQuery
                {
                    IsAnonymous = true,
                    PageSize = 1,
                });
                return query.Single();
            }

            var personQuery = await queryProcessor.Execute(new GetPeopleAsyncQuery
            {
                Emails = [command.Email],
                PageSize = 1,
            });
            var person = personQuery.SingleOrDefault();
            if (person != null)
                return person;

            try
            {
                var applicationUser = await commandProcessor.Execute(new CreateUserAsyncCommand
                {
                    Email = command.Email,
                    Password = randomGenerator.String(32),
                    PersonData = new CreateUserAsyncCommand.CreatePersonData
                    {
                        VatNumber = command.VatNumber,
                    },
                    OnEmailAlreadyExists = () => throw new Exception("This should never happen, because we already checked for existing email."),
                    OnInvadidEmail = () => throw new Exception("This should never happen, because we already checked for valid email."),
                    OnInvalidPassword = (passwordOptions) => throw new Exception("This should never happen, because we generate a random password."),
                });
                if (applicationUser == null)
                    throw new Exception("Failed to create user for guest charge.");

                return applicationUser.Person!;
            }
            catch
            {
                var query = await queryProcessor.Execute(new GetPeopleAsyncQuery
                {
                    IsAnonymous = true,
                    PageSize = 1,
                });
                return query.Single();
            }
        }

        private Task StartAcquiring(TransactionInfo transactionInfo, PosCharge posCharge)
        {
            switch (transactionInfo.MerchantAcquirerConfiguration.ChargePartner)
            {
                case ChargePartner.Quivi: break;
                case ChargePartner.Paybyrd: break;
                default: throw new NotImplementedException();
            }
            return Task.CompletedTask;
        }
    }
}