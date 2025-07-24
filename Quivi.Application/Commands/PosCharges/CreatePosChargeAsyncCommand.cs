using Quivi.Application.Commands.Sessions;
using Quivi.Application.Extensions.Pos;
using Quivi.Application.Pos.Items;
using Quivi.Application.Queries.PosIntegrations;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.PosCharges
{
    public class CreatePosChargeAsyncCommand : ICommand<Task<PosCharge?>>
    {
        public int MerchantId { get; init; }
        public int ChannelId { get; init; }
        public int CustomChargeMethodId { get; init; }
        public string? VatNumber { get; init; }
        public string? Email { get; init; }
        public string? Observations { get; init; }
        public decimal Amount { get; init; }
        public IEnumerable<SessionItem>? Items { get; init; }
        public decimal Tip { get; init; }
        public int? LocationId { get; init; }

        public required Action OnIntegrationDoesNotAllowPayments { get; init; }
        public required Action OnNoActiveSession { get; init; }
        public required Action OnInvalidItems { get; init; }
    }

    public class CreatePosChargeAsyncCommandHandler : ICommandHandler<CreatePosChargeAsyncCommand, Task<PosCharge?>>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IPosChargesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IPosSyncService posSyncService;
        private readonly IEventService eventService;

        public CreatePosChargeAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                    IPosChargesRepository repository,
                                                    IDateTimeProvider dateTimeProvider,
                                                    IPosSyncService posSyncService,
                                                    IEventService eventService)
        {
            this.queryProcessor = queryProcessor;
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.posSyncService = posSyncService;
            this.eventService = eventService;
        }

        public async Task<PosCharge?> Handle(CreatePosChargeAsyncCommand command)
        {
            var integration = await ValidateIntegration(command);
            if (integration == null)
            {
                command.OnIntegrationDoesNotAllowPayments();
                return null;
            }
            var session = await GetAndValidatePaymentWithSession(command);
            if(session == null)
            {
                command.OnNoActiveSession();
                return null;
            }

            if(ValidateSessionItems(command, session) == false)
            {
                command.OnInvalidItems();
                return null;
            }

            var posCharge = await CreatePosCharge(command, session);
            if(posCharge == null)
            {
                command.OnInvalidItems();
                return null;
            }

            await eventService.Publish(new OnPosChargeOperationEvent
            {
                Id = posCharge.ChargeId,
                MerchantId = posCharge.MerchantId,
                ChannelId = posCharge.ChannelId,
                Operation = EntityOperation.Create,
            });
            await eventService.Publish(new OnPosChargeCapturedEvent
            {
                Id = posCharge.Id,
                MerchantId = posCharge.MerchantId,
                ChannelId = posCharge.ChannelId,
            });

            return posCharge;
        }

        private async Task<PosIntegration?> ValidateIntegration(CreatePosChargeAsyncCommand command)
        {
            var integrationsQuery = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                MerchantIds = [command.MerchantId],
                ChannelIds = [command.ChannelId],
                PageSize = 1,
            });
            var integration = integrationsQuery.SingleOrDefault();

            if (integration == null)
                return null;

            var strategy = posSyncService.Get(integration.IntegrationType);
            var settings = strategy?.ParseSyncSettings(integration);
            if (settings?.AllowsPayments != true)
                return null;
            
            return integration;
        }

        private async Task<Session?> GetAndValidatePaymentWithSession(CreatePosChargeAsyncCommand command)
        {
            var sessionsQuery = await queryProcessor.Execute(new GetSessionsAsyncQuery
            {
                MerchantIds = [command.MerchantId],
                ChannelIds = [command.ChannelId],
                LatestSessionsOnly = true,
                IncludeOrdersMenuItemsPosChargeInvoiceItems = true,
                Statuses = [SessionStatus.Ordering],
                PageSize = 1,
            });
            var session = sessionsQuery.SingleOrDefault();
            return session;
        }

        private static bool ValidateSessionItems(CreatePosChargeAsyncCommand command, Session session)
        {
            var sessionItems = session.Orders!.SelectMany(o => o.OrderMenuItems!).AsSessionItems();
            var paidItems = session.Orders!.SelectMany(o => o.OrderMenuItems!).SelectMany(o => o.PosChargeInvoiceItems!).AsSessionItems();

            if (command.Items != null)
            {
                if (command.Items.Any() != true)
                    return false;

                // Items not found
                var commandItems = command.Items.Join(sessionItems, x => x, x => x, (cmd, db) => new
                {
                    CommandItem = cmd,
                    SessionItem = db
                }, new SessionItemComparer());

                if (commandItems.Count() != command.Items.Count())
                    return false;

                // Quantity is greater then the items to pay
                const decimal quantityMargin = 0.000001M;
                if (commandItems.Any(x => x.CommandItem.Quantity > (x.SessionItem.Quantity + quantityMargin)))
                    return false;
            }
            else
            {
                var unpaidTotal = sessionItems.Sum(CalculateItemAmount) - paidItems.Sum(CalculateItemAmount);
                var unmatchedAmount = command.Amount - unpaidTotal;
                if (unmatchedAmount > 0)
                {
                    var sessionItemCount = sessionItems.Sum(r => r.Quantity);
                    // since quantity is rounded to two decimal cases,
                    // there may be a max difference of 0.01 per item being paid
                    var allowedUnmatchedAmount = 0.01m * sessionItemCount;
                    if (unmatchedAmount > allowedUnmatchedAmount)
                        return false;
                }
            }

            return true;
        }

        private static decimal CalculateItemAmount(SessionItem item) => item.Quantity == 0 ? 0 : item.Quantity * item.GetUnitPrice();

        private async Task<PosCharge?> CreatePosCharge(CreatePosChargeAsyncCommand command, Session session)
        {
            var now = dateTimeProvider.GetUtcNow();
            PosCharge posCharge = new PosCharge
            {
                MerchantId = command.MerchantId,
                ChannelId = command.ChannelId,
                Total = command.Amount + command.Tip,
                Payment = command.Amount,
                Tip = command.Tip,
                SurchargeFeeAmount = 0.0m,
                SessionId = session.Id,
                LocationId = command.LocationId,
                Email = command.Email,
                VatNumber = command.VatNumber,
                Observations = command.Observations,
                CaptureDate = now,
                Charge = new Charge
                {
                    ChargeMethod = ChargeMethod.Custom,
                    ChainedChargeId = null,
                    Status = ChargeStatus.Completed,
                    MerchantCustomCharge = new MerchantCustomCharge
                    {
                        CustomChargeMethodId = command.CustomChargeMethodId,
                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                },

                CreatedDate = now,
                ModifiedDate = now,
            };

            if(AddPaymentTypeInformation(command, session, posCharge, now) == false)
                return null;

            repository.Add(posCharge);
            await repository.SaveChangesAsync();

            return posCharge;
        }

        private bool AddPaymentTypeInformation(CreatePosChargeAsyncCommand command, Session session, PosCharge posCharge, DateTime now)
        {
            if (command.Items == null)
            {
                posCharge.Total = command.Amount;
                posCharge.Payment = command.Amount;
                return true;
            }

            var totalPrice = command.Items.Sum(CalculateItemAmount);
            posCharge.Total = totalPrice;
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
            foreach(var payingItem in command.Items)
            {
                var quantity = payingItem.Quantity;
                if (allAvailableItems.TryGetValue(payingItem, out var availableItems) == false)
                    return false;

                foreach(var availableItem in availableItems)
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
    }
}
