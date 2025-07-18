﻿using Quivi.Application.Commands.PosChargeSyncAttempts;
using Quivi.Application.Commands.Sessions;
using Quivi.Application.Extensions.Pos;
using Quivi.Application.Pos;
using Quivi.Application.Pos.Items;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos.Commands;
using Quivi.Infrastructure.Abstractions.Pos.Exceptions;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Commands.Pos
{
    public class ProcessQuiviSyncChargeAsyncCommand : ICommand<Task<IEnumerable<IEvent>>>
    {
        public int PosChargeId { get; init; }
        public required AQuiviSyncStrategy SyncStrategy { get; init; }
    }

    public class ProcessQuiviSyncChargeAsyncCommandHandler : ASyncCommandHandler<ProcessQuiviSyncChargeAsyncCommand>
    {
        private class ExtendedOrderMenuItem
        {
            public required OrderMenuItem OrderMenuItem { get; init; }
            public decimal PaidQuantity { get; init; }
            public decimal AvailableQuantity => OrderMenuItem.Quantity - PaidQuantity;
            public required SessionItem SessionItem { get; init; }
        }


        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IPosChargesRepository posChargesRepository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IBackgroundJobHandler backgroundJobHandler;

        public ProcessQuiviSyncChargeAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                            ICommandProcessor commandProcessor,
                                                            IPosChargesRepository posChargesRepository,
                                                            IBackgroundJobHandler backgroundJobHandler,
                                                            IDateTimeProvider dateTimeProvider)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.dateTimeProvider = dateTimeProvider;
            this.backgroundJobHandler = backgroundJobHandler;
            this.posChargesRepository = posChargesRepository;
        }

        protected override async Task Sync(ProcessQuiviSyncChargeAsyncCommand command)
        {
            await commandProcessor.Execute(new UpsertPosChargeSyncAttemptAsyncCommand
            {
                Criteria = new GetPosChargeSyncAttemptsCriteria
                {
                    PosChargeIds = [command.PosChargeId],
                    States = [SyncAttemptState.Syncing, SyncAttemptState.Synced],
                    PageSize = 1,
                },
                UpdateAction = async e =>
                {
                    if (e.State != SyncAttemptState.Syncing)
                        return;

                    var posChargesQuery = await posChargesRepository.GetAsync(new GetPosChargesCriteria
                    {
                        Ids = [command.PosChargeId],
                        IncludePosChargeSelectedMenuItems = true,
                    });

                    var posCharge = posChargesQuery.Single();
                    if (posCharge.CaptureDate.HasValue == false)
                        throw new Exception($"Trying to process an invoice for an incompleted PosCharge with Id {command.PosChargeId}");

                    decimal paymentAmount = await ProcessPayment(command, posCharge);

                    e.State = SyncAttemptState.Synced;
                    e.SyncedAmount = paymentAmount;
                },
            });
        }

        private async Task<decimal> ProcessPayment(ProcessQuiviSyncChargeAsyncCommand command, PosCharge posCharge)
        {
            Session session = await GetSession(posCharge);

            var itemsToBePaid = GetAndProcessPayingItems(posCharge, session);
            decimal paymentAmount = Math.Round(itemsToBePaid.Sum(p => p.Item.FinalPrice * p.Quantity), maxDecimalPlaces);

            if (HasPendingItems(session) == false)
            {
                session.Status = SessionStatus.Closed;
                session.EndDate = dateTimeProvider.GetUtcNow();
            }

            await posChargesRepository.SaveChangesAsync();
            ProcessInvoice(command, posCharge, paymentAmount, itemsToBePaid);
            return paymentAmount;
        }

        private async Task<Session> GetSession(PosCharge posCharge)
        {
            var sessionsQuery = await queryProcessor.Execute(new GetSessionsAsyncQuery
            {
                Ids = [posCharge.SessionId!.Value],
                IncludeOrdersMenuItemsPosChargeInvoiceItems = true,
                IncludeOrdersMenuItemsModifiers = true,
                PageSize = 1,
            });
            return sessionsQuery.Single();
        }

        private bool HasPendingItems(Session session)
        {
            var sessionItems = session.Orders!.SelectMany(o => o.OrderMenuItems!).AsPaidSessionItems();
            foreach (var model in sessionItems)
            {
                var unpaidQuantity = model.Quantity - model.PaidQuantity;
                if (unpaidQuantity > 0)
                    return true;
            }
            return false;
        }

        private IEnumerable<(OrderMenuItem Item, decimal Quantity)> GetAndProcessPayingItems(PosCharge posCharge, Session session)
        {
            if (session.Status == SessionStatus.Unknown)
                throw new PosPaymentSyncingException(SyncingState.SessionDoesNotExist, $"Charge {posCharge.ChargeId} cannot be sent to PoS since the session {session.Id} is deleted.");

            if (session.Status == SessionStatus.Closed)
                throw new PosPaymentSyncingException(SyncingState.SessionAlreadyClosed, $"Charge {posCharge.ChargeId} cannot be sent to PoS since the session {session.Id} is already closed.");

            IEnumerable<(OrderMenuItem PayingItem, decimal Quantity)> payingItems = GetPayingItems(posCharge, session);
            var now = dateTimeProvider.GetUtcNow();

            var orderMenuItems = session.Orders!.SelectMany(o => o.OrderMenuItems!).ToDictionary(e => e.Id, e => e);
            posCharge.PosChargeInvoiceItems = new List<PosChargeInvoiceItem>();
            foreach(var s in payingItems)
            {
                var item = new PosChargeInvoiceItem
                {
                    PosCharge = posCharge,
                    Quantity = s.Quantity,
                    OrderMenuItemId = s.PayingItem.Id,
                    CreatedDate = now,
                    ModifiedDate = now,
                    ChildrenPosChargeInvoiceItems = new List<PosChargeInvoiceItem>(),
                };
                foreach(var e in s.PayingItem.Modifiers!)
                {
                    var extra = new PosChargeInvoiceItem
                    {
                        PosCharge = posCharge,
                        Quantity = e.Quantity,
                        OrderMenuItemId = e.Id,
                        CreatedDate = now,
                        ModifiedDate = now,
                    };
                    item.ChildrenPosChargeInvoiceItems.Add(extra);
                    posCharge.PosChargeInvoiceItems.Add(extra);

                    orderMenuItems[extra.OrderMenuItemId].PosChargeInvoiceItems!.Add(extra);
                }
                posCharge.PosChargeInvoiceItems.Add(item);
                orderMenuItems[item.OrderMenuItemId].PosChargeInvoiceItems!.Add(item);
            }

            this.AddSessionEvent(session, s => new OnPosChargeSyncedEvent
            {
                ChannelId = session.ChannelId,
                Id = posCharge.Id,
                MerchantId = posCharge.MerchantId,
                SessionId = session.Id,
            });
            return payingItems;
        }

        private IEnumerable<(OrderMenuItem ItemPaid, decimal Quantity)> GetPayingItems(PosCharge charge, Session session)
        {
            var comparer = new SessionItemComparer();
            var availableItems = session.Orders!.SelectMany(o => o.OrderMenuItems!)
                                                .Select(omi => new ExtendedOrderMenuItem
                                                {
                                                    OrderMenuItem = omi,
                                                    PaidQuantity = omi.PosChargeInvoiceItems!.Sum(ii => ii.Quantity),
                                                    SessionItem = omi.AsSessionItem(),
                                                })
                                                .GroupBy(e => e.SessionItem, comparer)
                                                .Select(g => new
                                                {
                                                    TotalPaidQuantity = g.Sum(s => s.PaidQuantity),
                                                    TotalQuantity = g.Sum(s => s.OrderMenuItem.Quantity),
                                                    Items = g.AsEnumerable(),
                                                }).Where(e => e.TotalQuantity > e.TotalPaidQuantity)
                                                .SelectMany(e => e.Items)
                                                .ToList();

            Dictionary<int, ExtendedOrderMenuItem> availableItemsDictionary = new Dictionary<int, ExtendedOrderMenuItem>();
            IList<(OrderMenuItem ItemPaid, decimal Quantity)> itemsWithNoValue = new List<(OrderMenuItem ItemPaid, decimal Quantity)>();
            foreach (var pendingItem in availableItems)
            {
                if (pendingItem.SessionItem.GetUnitPrice(maxDecimalPlaces) > 0)
                {
                    availableItemsDictionary.Add(pendingItem.OrderMenuItem.Id, pendingItem);
                    continue;
                }

                itemsWithNoValue.Add((pendingItem.OrderMenuItem, pendingItem.AvailableQuantity));
            }

            var items = charge.PosChargeSelectedMenuItems!.Any() == true ? GetPayingItemsFromUsersSelection(availableItemsDictionary, charge) : GetPayingItemsFromFreePayment(availableItemsDictionary, charge);
            return itemsWithNoValue.Concat(items);
        }

        private IEnumerable<(OrderMenuItem ItemPaid, decimal Quantity)> GetPayingItemsFromUsersSelection(IReadOnlyDictionary<int, ExtendedOrderMenuItem> availableItems, PosCharge posCharge)
        {
            const decimal quantityMargin = 0.000001M;

            IList<(OrderMenuItem, decimal)> result = new List<(OrderMenuItem, decimal)>();
            foreach (var selectedItem in posCharge.PosChargeSelectedMenuItems!)
            {
                decimal quantity = selectedItem.Quantity;

                if (availableItems.TryGetValue(selectedItem.OrderMenuItemId, out var orderedItem))
                {
                    decimal availableQuantity = orderedItem.AvailableQuantity;
                    if (availableQuantity == 0)
                        return GetPayingItemsFromFreePayment(availableItems, posCharge);

                    decimal quantityToTake = quantity - availableQuantity >= 0 ? availableQuantity : quantity;

                    quantity -= quantityToTake;
                    result.Add((orderedItem.OrderMenuItem, quantityToTake));
                }

                if (Math.Abs(quantity) > quantityMargin)
                    return GetPayingItemsFromFreePayment(availableItems, posCharge);
            }
            return result;
        }

        private IEnumerable<(OrderMenuItem ItemPaid, decimal Quantity)> GetPayingItemsFromFreePayment(IReadOnlyDictionary<int, ExtendedOrderMenuItem> availableItems, PosCharge posCharge)
        {
            const decimal quantityMargin = 0.000001M;

            IList<(OrderMenuItem, decimal)> result = new List<(OrderMenuItem, decimal)>();
            decimal amountToTakeRemaining = posCharge.Payment;
            foreach (var item in availableItems.Values)
            {
                decimal availableQuantity = item.AvailableQuantity;
                if (availableQuantity == 0)
                    continue;

                decimal itemUnitPrice = item.SessionItem.GetUnitPrice(maxDecimalPlaces);
                decimal quantityToTake = itemUnitPrice * availableQuantity <= amountToTakeRemaining ? availableQuantity : amountToTakeRemaining / itemUnitPrice;
                decimal amountToTake = quantityToTake * itemUnitPrice;

                amountToTakeRemaining -= amountToTake;
                result.Add((item.OrderMenuItem, quantityToTake));

                if (Math.Abs(amountToTakeRemaining) <= quantityMargin)
                    return result;
            }
            return result;
        }

        private void ProcessInvoice(ProcessQuiviSyncChargeAsyncCommand command, PosCharge posCharge, decimal paymentAmount, IEnumerable<(OrderMenuItem, decimal)> itemsToBePaid)
        {
            var itemsToBePaidData = itemsToBePaid.Select(i => new
            {
                OrderMenuItem = i.Item1,
                QuantityToBePaid = i.Item2,
            });

            List<AQuiviSyncStrategy.InvoiceItem> items = itemsToBePaidData.Select(i => new AQuiviSyncStrategy.InvoiceItem
            {
                MenuItemId = i.OrderMenuItem.MenuItemId,
                Name = i.OrderMenuItem.Name,
                UnitPrice = i.OrderMenuItem.OriginalPrice,
                VatRate = i.OrderMenuItem.VatRate,
                Quantity = i.QuantityToBePaid,
                DiscountPercentage = PriceHelper.CalculateDiscountPercentage(i.OrderMenuItem.OriginalPrice, i.OrderMenuItem.FinalPrice),
            }).ToList();

            //TODO: Move the method ProcessInvoiceJob inside this command
            var posChargeId = posCharge.Id;
            backgroundJobHandler.Enqueue(() => command.SyncStrategy.ProcessInvoiceJob(posChargeId, paymentAmount, items));
        }
    }
}
