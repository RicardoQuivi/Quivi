using Quivi.Application.Queries.Postings;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Services.Charges;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Services
{
    public class ChargeProcessor : IChargeProcessor
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IChargesRepository chargesRepository;
        private readonly IJournalsRepository journalsRepository;

        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEnumerable<IAcquirerProcessingStrategy> acquirerStrategies;
        private readonly IEventService eventService;
        private readonly IBackgroundJobHandler backgroundJobHandler;
        private readonly IIdConverter idConverter;
        private readonly IPosSyncService posSyncService;
        private readonly IQueryProcessor queryProcessor;

        public ChargeProcessor(IUnitOfWork unitOfWork,
                                    IEnumerable<IAcquirerProcessingStrategy> acquirerStrategies,
                                    IDateTimeProvider dateTimeProvider,
                                    IEventService eventService,
                                    IBackgroundJobHandler backgroundJobHandler,
                                    IIdConverter idConverter,
                                    IPosSyncService posSyncService,
                                    IQueryProcessor queryProcessor)
        {
            this.unitOfWork = unitOfWork;
            this.chargesRepository = unitOfWork.Charges;
            this.journalsRepository = unitOfWork.Journals;

            this.acquirerStrategies = acquirerStrategies;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
            this.backgroundJobHandler = backgroundJobHandler;
            this.idConverter = idConverter;
            this.posSyncService = posSyncService;
            this.queryProcessor = queryProcessor;
        }

        #region IChargeProcessor
        public async Task<StartProcessingResult> StartProcessing(int chargeId, Action<Charge> onProcessingStart)
        {
            string? challengeUrl = null;
            var charge = await ProcessChargeWithTransaction(chargeId, async (c) =>
            {
                bool hasChanges = false;

                if (c.Status == ChargeStatus.Requested)
                {
                    challengeUrl = await StartCharge(c, onProcessingStart);
                    hasChanges = true;
                }

                if (c.Status == ChargeStatus.Processing)
                {
                    var status = await CheckAndUpdateState(c);
                    if (status == PaymentStatus.Failed || status == PaymentStatus.Success)
                        hasChanges = true;
                }

                return hasChanges;
            });

            if (charge == null)
                return new StartProcessingResult();

            if (charge.Status == ChargeStatus.Processing)
                SchedulePolling(charge.Id);

            return new StartProcessingResult
            {
                Charge = charge,
                ChallengeUrl = challengeUrl,
            };
        }

        public Task CheckAndUpdateState(int chargeId) => Polling(chargeId, false);

        public async Task Refund(RefundParameters parameters)
        {
            var validatedDetails = await GetAndValidateChargeForRefund(parameters);
            if (validatedDetails == null)
                return;

            var posCharge = await ProcessRefund(parameters, validatedDetails.Value.Charge, validatedDetails.Value.TotalRefundAmount, validatedDetails.Value.TipRefundAmount);
            await unitOfWork.SaveChangesAsync();

            await eventService.Publish(new OnPosChargeRefundedEvent
            {
                Id = posCharge.Id,
                ChannelId = posCharge.ChannelId,
                MerchantId = posCharge.MerchantId,
            });
        }
        #endregion

        #region Processing Charge
        private async Task<Charge?> GetCharge(int chargeId)
        {
            var chargesQuery = await chargesRepository.GetAsync(new GetChargesCriteria
            {
                Ids = [chargeId],

                IncludePosCharge = true,
                IncludeCardCharge = true,
                IncludeMerchantAcquirerConfiguration = true,

                IncludeDeposit = true,
                IncludeDepositDepositSurchage = true,
                IncludeDepositDepositCapture = true,
                IncludeDepositCaptureJournal = true,
                IncludeDepositDepositSurcharge = true,
                IncludeDepositSurchargeJournal = true,
                IncludeDepositConsumer = true,
                IncludeChainedCharge = true,
                IncludeDepositDepositCapturePerson = true,
            });
            var charge = chargesQuery.SingleOrDefault();
            return charge;
        }

        private async Task<Charge?> ProcessChargeWithTransaction(int chargeId, Func<Charge, Task<bool>> action)
        {
            var charge = await GetCharge(chargeId);
            if (charge == null)
                return null;

            await using var transaction = await unitOfWork.StartTransactionAsync();

            var hasChanges = await action(charge);
            if (hasChanges == false)
            {
                await transaction.RollbackAsync();
                return charge;
            }

            await chargesRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            await eventService.Publish(new OnPosChargeOperationEvent
            {
                Id = charge.PosCharge!.Id,
                ChannelId = charge.PosCharge!.ChannelId,
                MerchantId = charge.PosCharge!.MerchantId,
                Operation = EntityOperation.Update,
            });

            if (charge.PosCharge.CaptureDate.HasValue)
                await eventService.Publish(new OnPosChargeCapturedEvent
                {
                    Id = charge.Id,
                    MerchantId = charge.PosCharge!.MerchantId,
                    ChannelId = charge.PosCharge!.ChannelId,
                });

            return charge;
        }

        private async Task<string?> StartCharge(Charge charge, Action<Charge> onProcessingStart)
        {
            string? challengeUrl = null;

            var strategy = acquirerStrategies.FirstOrDefault(c => c.ChargePartner == charge.ChargePartner && c.ChargeMethod == charge.ChargeMethod);
            if (strategy == null)
            {
                charge.Status = ChargeStatus.Failed;
                charge.ModifiedDate = dateTimeProvider.GetUtcNow();
                return challengeUrl;
            }

            try
            {
                onProcessingStart(charge);
                var processingResult = await strategy.Process(charge);

                var now = dateTimeProvider.GetUtcNow();
                if (charge.ChargeMethod == ChargeMethod.CreditCard)
                {
                    if (processingResult.CreditCard == null)
                    {
                        charge.Status = ChargeStatus.Failed;
                        charge.ModifiedDate = now;
                        return challengeUrl;
                    }
                    challengeUrl = processingResult.CreditCard.ChallengeUrl;

                    if (charge.CardCharge != null)
                        charge.CardCharge.TransactionId = processingResult.GatewayTransactionId;
                    else
                        charge.CardCharge = new CardCharge
                        {
                            AuthorizationToken = string.Empty,
                            FormContext = string.Empty,
                            TransactionId = processingResult.GatewayTransactionId,

                            Charge = charge,
                            ChargeId = charge.Id,

                            CreatedDate = now,
                            ModifiedDate = now,
                        };
                }

                charge.Status = ChargeStatus.Processing;
                charge.ModifiedDate = now;
            }
            catch
            {
                charge.Status = ChargeStatus.Failed;
                charge.ModifiedDate = dateTimeProvider.GetUtcNow();
            }
            return challengeUrl;
        }

        private async Task<PaymentStatus> CheckAndUpdateState(Charge charge)
        {
            var strategy = acquirerStrategies.FirstOrDefault(c => c.ChargePartner == charge.ChargePartner && c.ChargeMethod == charge.ChargeMethod);
            var status = await (strategy?.GetStatus(charge) ?? Task.FromResult(PaymentStatus.Failed));

            if (status == PaymentStatus.Failed)
            {
                charge.Status = ChargeStatus.Failed;
                charge.ModifiedDate = dateTimeProvider.GetUtcNow();
            }
            else if (status == PaymentStatus.Success)
                await ProcessAcquirerCompletedCharge(charge);

            return status;
        }

        private void SchedulePolling(int chargeId) => backgroundJobHandler.Schedule(() => Polling(chargeId, true), TimeSpan.FromSeconds(15));

        public async Task Polling(int chargeId, bool continuePolling)
        {
            try
            {
                await ProcessChargeWithTransaction(chargeId, async (c) =>
                {
                    var chargeInitialState = c.Status;
                    var status = await CheckAndUpdateState(c);
                    if (status != PaymentStatus.Processing)
                    {
                        continuePolling = false;
                        return true;
                    }

                    return false;
                });
            }
            catch
            {
            }

            if (continuePolling)
                SchedulePolling(chargeId);
        }
        #endregion

        #region Complete Charge
        private async Task ProcessAcquirerCompletedCharge(Charge charge)
        {
            if (charge.Status != ChargeStatus.Processing)
                return;

            if (charge.Deposit!.DepositCaptureJournal != null)
                return;

            if (charge.Deposit!.DepositSurchargeJournal != null)
                return;

            var quiviPerson = await GetQuiviPerson();

            var tip = charge.PosCharge!.Tip;
            var now = dateTimeProvider.GetUtcNow();

            ProcessConsumerJournal(charge, quiviPerson, tip, now);
            ProcessCaptureJournal(charge, charge.Deposit!.DepositCapture?.Amount ?? 0, tip, now);
            ProcessSurchargeJournal(quiviPerson, charge, charge.Deposit!.DepositSurchage?.Amount ?? 0.0M, now);

            if (charge.ChargeMethod == ChargeMethod.MbWay && charge.MbWayCharge != null)
                charge.PosCharge!.PhoneNumber ??= charge.MbWayCharge.PhoneNumber;

            charge.PosCharge!.CaptureDate = now;
            charge.Status = ChargeStatus.Completed;
        }

        private void ProcessConsumerJournal(Charge charge, Person quiviPerson, decimal tip, DateTime now)
        {
            if (charge.ChargeMethod == ChargeMethod.Wallet || charge.Deposit!.DepositCapture!.Amount <= 0)
                return;

            var amount = charge.Deposit.Amount;
            var journal = new Journal
            {
                State = JournalState.Completed,
                Type = JournalType.Deposit,
                OrderRef = idConverter.ToPublicId(charge.Id),
                JournalDetails = tip <= 0.0M ? null : new JournalDetails
                {
                    IncludedTip = tip,
                    CreatedDate = now,
                    ModifiedDate = now,
                },
                Postings = new List<Posting>
                {
                    new Posting
                    {
                        Amount = amount,
                        AssetType = "EUR",
                        PersonId = charge.Deposit.ConsumerId,

                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                    new Posting
                    {
                        Amount = amount * -1,
                        AssetType = "EUR",
                        PersonId = quiviPerson.Id,

                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                },
                DepositJournals = new List<DepositJournal>
                {
                    new DepositJournal
                    {
                        DepositId = charge.Id,
                    },
                },
                CreatedDate = now,
                ModifiedDate = now,
            };
            journal.JournalChanges = new List<JournalChange>
            {
                new JournalChange
                {
                    State = JournalState.Completed,
                    Type = JournalType.Deposit,
                    Amount = amount,

                    Journal = journal,

                    CreatedDate = now,
                    ModifiedDate = now,
                }
            };
            unitOfWork.Journals.Add(journal);
        }

        private void ProcessCaptureJournal(Charge charge, decimal captureAmount, decimal tip, DateTime now)
        {
            if (charge.IsTopUp())
                return;

            if (captureAmount == 0)
                return;

            var journal = new Journal
            {
                State = JournalState.Completed,
                Type = charge.Deposit!.DepositCapture!.Type,
                OrderRef = idConverter.ToPublicId(charge.Id),
                Method = JournalMethod.Integrated,
                ChargeMethod = charge.ChargeMethod,
                JournalDetails = tip > 0.0M ? new JournalDetails
                {
                    IncludedTip = tip,
                } : null,
                Postings = new List<Posting>
                {
                    new Posting
                    {
                        Amount = captureAmount * -1,
                        AssetType = "EUR",
                        PersonId = charge.Deposit.ConsumerId,

                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                    new Posting
                    {
                        Amount = captureAmount,
                        AssetType = "EUR",
                        PersonId = charge.Deposit.DepositCapture.PersonId,

                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                },
                DepositCaptureJournals = new List<DepositCaptureJournal>
                {
                    new DepositCaptureJournal
                    {
                        DepositId = charge.Id,
                    },
                },
                CreatedDate = now,
                ModifiedDate = now,
            };
            journal.JournalChanges = new List<JournalChange>
            {
                new JournalChange
                {
                    State = JournalState.Completed,
                    Type = charge.Deposit!.DepositCapture!.Type,
                    Amount = captureAmount,

                    Journal = journal,

                    CreatedDate = now,
                    ModifiedDate = now,
                },
            };
            unitOfWork.Journals.Add(journal);
        }

        private void ProcessSurchargeJournal(Person quiviPerson, Charge charge, decimal surcharge, DateTime now)
        {
            if (charge.IsTopUp())
                return;

            if (surcharge <= 0)
                return;

            var journal = new Journal
            {
                State = JournalState.Completed,
                Type = JournalType.Surcharge,
                OrderRef = idConverter.ToPublicId(charge.Id),
                Method = JournalMethod.Integrated,
                ChargeMethod = charge.ChargeMethod,
                Postings = new List<Posting>
                {
                    new Posting
                    {
                        Amount = surcharge * -1,
                        AssetType = "EUR",
                        PersonId = charge.Deposit!.ConsumerId,

                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                    new Posting
                    {
                        Amount = surcharge,
                        AssetType = "EUR",
                        PersonId = quiviPerson.Id,

                        CreatedDate = now,
                        ModifiedDate = now,
                    }
                },
                DepositSurchargeJournals = new List<DepositSurchargeJournal>
                {
                    new DepositSurchargeJournal
                    {
                        DepositId = charge.Id,
                    },
                },
                CreatedDate = now,
                ModifiedDate = now,
            };
            journal.JournalChanges = new List<JournalChange>
            {
                new JournalChange
                {
                    State = JournalState.Completed,
                    Type = JournalType.Surcharge,
                    Amount = surcharge,

                    CreatedDate = now,
                    ModifiedDate = now,

                    Journal = journal,
                }
            };
            unitOfWork.Journals.Add(journal);
        }
        #endregion

        #region Refund
        public async Task<(Charge Charge, decimal TotalRefundAmount, decimal TipRefundAmount)?> GetAndValidateChargeForRefund(RefundParameters parameters)
        {
            if (parameters.Amount.HasValue && parameters.Amount.Value <= 0)
                return null;

            var chargesQuery = await chargesRepository.GetAsync(new GetChargesCriteria
            {
                MerchantIds = parameters.MerchantId.HasValue ? [parameters.MerchantId.Value] : null,
                Ids = [parameters.ChargeId],
                Statuses = [ChargeStatus.Completed],

                IncludePosCharge = true,
                IncludePosChargeMerchant = true,
                IncludeDeposit = true,
                IncludeDepositCaptureJournal = true,
                IncludeDepositCaptureJournalPostingsPerson = true,
                IncludeDepositCaptureJournalChanges = true,
                IncludeDepositDepositCapture = true,
                IncludeMerchantAcquirerConfiguration = true,
            });

            var charge = chargesQuery.SingleOrDefault();
            if (charge == null)
                return null;

            if (charge.PosCharge!.PaymentRefund > 0)
                return null;

            var capturedAmount = charge.ChargeMethod == ChargeMethod.Custom ? charge.PosCharge.Total : charge.Deposit!.DepositCapture!.Amount;
            if (capturedAmount <= 0)
                return null;

            var availableAmount = capturedAmount;
            var refundAmount = parameters.Amount ?? availableAmount;

            if (charge.ChargeMethod != ChargeMethod.Custom && await GetChannelBalanceSinceLastSettlement(charge.Deposit!.DepositCapture!.PersonId, charge.PosCharge.Merchant!) < refundAmount)
                throw new Exception(); //TODO: Throw a catchable Exception

            if (parameters.IsCancellation)
            {
                if (charge.PosCharge!.CaptureDate!.Value < dateTimeProvider.GetUtcNow().AddDays(-1))
                    throw new Exception(); //TODO: Throw a catchable Exception

                if (refundAmount != charge.PosCharge.Total)
                    throw new Exception(); //TODO: Throw a catchable Exception
            }

            if (!await posSyncService.CanRefundCharge(parameters.ChargeId, refundAmount, parameters.IsCancellation ? InvoiceRefundType.Cancellation : InvoiceRefundType.CreditNote))
                throw new NotSupportedException();

            var originalTip = charge.PosCharge.Tip;
            var originalPayment = charge.PosCharge.Payment;
            var tipRefund = (originalTip > 0.00M && refundAmount > originalPayment) ? refundAmount - originalPayment : 0;

            return (charge, refundAmount, tipRefund);
        }

        private async Task<PosCharge> ProcessRefund(RefundParameters parameters, Charge charge, decimal totalAmountToRefund, decimal tipRefundAmount)
        {
            var now = dateTimeProvider.GetUtcNow();
            if (charge.ChargeMethod != ChargeMethod.Custom)
            {
                var strategy = acquirerStrategies.FirstOrDefault(c => c.ChargePartner == charge.ChargePartner && c.ChargeMethod == charge.ChargeMethod);
                if (strategy == null)
                    throw new NotSupportedException();

                var originalJournal = charge.Deposit!.DepositCaptureJournal!.Journal!;
                ProcessMerchantJournal(charge, now, originalJournal, totalAmountToRefund, tipRefundAmount);
                await ProcessQuiviJournal(charge, now, totalAmountToRefund);
                await strategy.Refund(charge, totalAmountToRefund);
            }

            charge.PosCharge!.PaymentRefund = totalAmountToRefund - tipRefundAmount;
            charge.PosCharge!.TipRefund = tipRefundAmount;
            charge.PosCharge!.InvoiceRefundType = parameters.IsCancellation ? InvoiceRefundType.Cancellation : InvoiceRefundType.CreditNote;
            charge.PosCharge!.RefundReason = parameters.Reason;
            charge.PosCharge!.RefundEmployeeId = parameters.EmployeeId;
            charge.PosCharge!.TotalRefund = charge.PosCharge!.PaymentRefund + charge.PosCharge!.TipRefund;

            return charge.PosCharge;
        }

        private void ProcessMerchantJournal(Charge charge, DateTime now, Journal originalJournal, decimal refundAmount, decimal tipRefund)
        {
            var originalPostingConsumer = originalJournal.Postings!.First(p => p.Person!.PersonType == PersonType.Consumer);
            var originalPostingChannel = originalJournal.Postings!.First(p => p.Person!.PersonType == PersonType.Channel);

            var journal = new Journal
            {
                State = JournalState.Completed,
                Type = JournalType.Refund,
                OrderRef = originalJournal.OrderRef,
                Method = originalJournal.Method,
                JournalLinkId = originalJournal.Id,
                ChargeMethod = originalJournal.ChargeMethod,
                DepositRefundJournals = new List<DepositRefundJournal>
                {
                    new DepositRefundJournal
                    {
                        DepositId = charge.Id,

                        CreatedDate = now,
                        ModifiedDate = now,
                    }
                },
                JournalDetails = tipRefund > 0 ? new JournalDetails
                {
                    IncludedTip = tipRefund,

                    CreatedDate = now,
                    ModifiedDate = now,
                } : null,
                JournalChanges = new List<JournalChange>(),

                CreatedDate = now,
                ModifiedDate = now,
            };
            journal.Postings = new List<Posting>
            {
                new Posting
                {
                    Amount = refundAmount,
                    AssetType = originalPostingConsumer.AssetType,
                    PersonId = originalPostingConsumer.PersonId,
                    Journal = journal,

                    CreatedDate = now,
                    ModifiedDate = now,
                },
                new Posting
                {
                    Amount = refundAmount * -1,
                    AssetType = originalPostingChannel.AssetType,
                    PersonId = originalPostingChannel.PersonId,
                    Journal = journal,

                    CreatedDate = now,
                    ModifiedDate = now,
                },
            };

            journalsRepository.Add(journal);

            originalJournal.JournalChanges!.Add(new JournalChange
            {
                State = originalJournal.State,
                Type = originalJournal.Type,
                Amount = refundAmount,

                JournalLink = journal,
                JournalLinkId = journal.Id,

                JournalId = originalJournal.Id,
                Journal = originalJournal,

                CreatedDate = now,
                ModifiedDate = now,
            });

            journal.JournalChanges.Add(new JournalChange
            {
                State = journal.State,
                Type = journal.Type,
                Amount = refundAmount,

                JournalLink = originalJournal,
                JournalLinkId = originalJournal.Id,

                JournalId = journal.Id,
                Journal = journal,

                CreatedDate = now,
                ModifiedDate = now,
            });
        }

        private async Task ProcessQuiviJournal(Charge charge, DateTime now, decimal totalAmountToRefund)
        {
            var quiviPerson = await GetQuiviPerson();

            var journal = new Journal
            {
                State = JournalState.Completed,
                Type = JournalType.Withdrawal,
                OrderRef = idConverter.ToPublicId(charge.Id),
                JournalDetails = null,
                CreatedDate = now,
                ModifiedDate = now,
            };
            journal.Postings = new List<Posting>
            {
                new Posting
                {
                    Amount = totalAmountToRefund,
                    AssetType = "EUR",
                    PersonId = charge.Deposit!.Consumer!.Id,
                    Journal = journal,

                    CreatedDate = now,
                    ModifiedDate = now,
                },
                new Posting
                {
                    Amount = totalAmountToRefund * -1,
                    AssetType = "EUR",
                    PersonId = quiviPerson.Id,
                    Journal = journal,

                    CreatedDate = now,
                    ModifiedDate = now,
                },
            };

            journal.JournalChanges = new List<JournalChange>
            {
                new JournalChange
                {
                    CreatedDate = journal.CreatedDate,
                    State = journal.State,
                    Type = journal.Type,
                    Amount = totalAmountToRefund,
                    JournalLinkId = journal.JournalLinkId,
                    Journal = journal,
                }
            };

            journalsRepository.Add(journal);
        }

        private async Task<decimal> GetChannelBalanceSinceLastSettlement(int personId, Merchant merchant)
        {
            var fromDateUtc = dateTimeProvider.GetUtcNow().GetSettlementFromDateUtc(merchant.TimeZone);
            var balances = await queryProcessor.Execute(new GetAccountsBalanceAsyncQuery
            {
                PersonIds = [personId],
                FromDate = fromDateUtc,
                JournalTypes = Enum.GetValues(typeof(JournalType))
                                    .Cast<JournalType>()
                                    .Except(
                                    [
                                        JournalType.AccountingClosing,
                                        JournalType.TransactionFees,
                                        JournalType.MerchantBilling,
                                        JournalType.MerchantBillingVat,
                                        JournalType.MerchantReimbursement,
                                    ]),
            });

            return balances.DefaultIfEmpty()?.Single().Value ?? 0m;
        }
        #endregion

        private async Task<Person> GetQuiviPerson()
        {
            var quiviPersonQuery = await unitOfWork.People.GetAsync(new GetPeopleCriteria
            {
                PersonTypes = [PersonType.Quivi],
                PageSize = 1,
            });
            var quiviPerson = quiviPersonQuery.Single();
            return quiviPerson;
        }
    }
}