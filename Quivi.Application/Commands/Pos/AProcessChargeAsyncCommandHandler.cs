using Quivi.Application.Extensions;
using Quivi.Application.Extensions.Pos;
using Quivi.Application.Pos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Pos.Commands;
using Quivi.Infrastructure.Abstractions.Pos.Exceptions;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Commands.Pos
{
    public abstract class AProcessChargeAsyncCommandHandler<TCommand> : ASyncCommandHandler<TCommand> where TCommand : ICommand<Task<IEnumerable<IEvent>>>
    {
        const decimal quantityMargin = 0.000001M;

        private class ExtendedOrderMenuItem
        {
            public required OrderMenuItem OrderMenuItem { get; init; }
            public decimal PaidQuantity { get; init; }
            public decimal AvailableQuantity => OrderMenuItem.Quantity - PaidQuantity;
            public required SessionItem SessionItem { get; init; }
        }

        private class GrouppedOrderMenuItem
        {
            public decimal TotalPaidQuantity { get; init; }
            public decimal TotalQuantity { get; init; }
            public decimal AvailableQuantity => TotalQuantity - TotalPaidQuantity;

            public required SessionItem SessionItem { get; init; }
            public required IEnumerable<ExtendedOrderMenuItem> Items { get; init; }
        }


        protected readonly IDateTimeProvider dateTimeProvider;
        protected readonly IBackgroundJobHandler backgroundJobHandler;

        public AProcessChargeAsyncCommandHandler(IDateTimeProvider dateTimeProvider, IBackgroundJobHandler backgroundJobHandler)
        {
            this.dateTimeProvider = dateTimeProvider;
            this.backgroundJobHandler = backgroundJobHandler;
        }

        protected async Task<decimal> ProcessPayment(AQuiviSyncStrategy syncStrategy, Session session, PosCharge posCharge, Func<Task> onComplete)
        {
            var itemsToBePaid = GetAndProcessPayingItems(posCharge, session);
            decimal paymentAmount = Math.Round(itemsToBePaid.Sum(p => p.Item.FinalPrice * p.Quantity), maxDecimalPlaces);

            if (HasPendingItems(session) == false)
            {
                session.Status = SessionStatus.Closed;
                session.EndDate = dateTimeProvider.GetUtcNow();
            }

            await onComplete();
            ProcessInvoice(syncStrategy, posCharge, paymentAmount, itemsToBePaid);
            return paymentAmount;
        }

        private IEnumerable<(OrderMenuItem Item, decimal Quantity)> GetAndProcessPayingItems(PosCharge posCharge, Session session)
        {
            if (session.Status == SessionStatus.Unknown)
                throw new PosPaymentSyncingException(SyncingState.SessionDoesNotExist, $"Charge {posCharge.ChargeId} cannot be sent to PoS since the session {session.Id} is deleted.");

            if (session.Status == SessionStatus.Closed)
                throw new PosPaymentSyncingException(SyncingState.SessionAlreadyClosed, $"Charge {posCharge.ChargeId} cannot be sent to PoS since the session {session.Id} is already closed.");

            IEnumerable<(OrderMenuItem PayingItem, decimal Quantity)> payingItems = GetPayingItems(posCharge, session);
            var now = dateTimeProvider.GetUtcNow();

            var orderMenuItems = session.GetValidOrderMenuItems().SelectMany(o => (o.Modifiers ?? []).Prepend(o)).ToDictionary(e => e.Id, e => e);
            posCharge.PosChargeInvoiceItems = new List<PosChargeInvoiceItem>();
            foreach (var payingItem in payingItems)
            {
                var item = new PosChargeInvoiceItem
                {
                    PosCharge = posCharge,
                    Quantity = payingItem.Quantity,
                    OrderMenuItemId = payingItem.PayingItem.Id,
                    CreatedDate = now,
                    ModifiedDate = now,
                    ChildrenPosChargeInvoiceItems = new List<PosChargeInvoiceItem>(),
                };
                foreach (var payingExtra in payingItem.PayingItem.Modifiers!)
                {
                    var extraQuantityPerParent = payingItem.PayingItem.Quantity == 0 ? 0 : payingExtra.Quantity / payingItem.PayingItem.Quantity;
                    var extra = new PosChargeInvoiceItem
                    {
                        PosCharge = posCharge,
                        Quantity = payingItem.Quantity * extraQuantityPerParent,
                        OrderMenuItemId = payingExtra.Id,
                        CreatedDate = now,
                        ModifiedDate = now,
                        ParentPosChargeInvoiceItem = item,
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
            var availableItems = session.GetValidOrderMenuItems()
                                        .Select(omi => new ExtendedOrderMenuItem
                                        {
                                            OrderMenuItem = omi,
                                            PaidQuantity = omi.PosChargeInvoiceItems?.Sum(ii => ii.Quantity) ?? 0.0m,
                                            SessionItem = omi.AsSessionItem(),
                                        })
                                        .GroupBy(e => e.SessionItem, comparer)
                                        .Select(g => new GrouppedOrderMenuItem
                                        {
                                            SessionItem = g.Key,
                                            TotalPaidQuantity = g.Sum(s => s.PaidQuantity),
                                            TotalQuantity = g.Sum(s => s.OrderMenuItem.Quantity),
                                            Items = g.AsEnumerable(),
                                        })
                                        .Where(e => e.TotalQuantity > e.TotalPaidQuantity)
                                        .ToList();


            Dictionary<int, GrouppedOrderMenuItem> availableItemsDictionary = new Dictionary<int, GrouppedOrderMenuItem>();
            List<(OrderMenuItem ItemPaid, decimal Quantity)> itemsWithNoValue = new List<(OrderMenuItem ItemPaid, decimal Quantity)>();
            foreach (var pendingItem in availableItems)
            {
                if (pendingItem.SessionItem.GetUnitPrice(maxDecimalPlaces) > 0)
                {
                    foreach (var item in pendingItem.Items)
                        availableItemsDictionary.Add(item.OrderMenuItem.Id, pendingItem);
                    continue;
                }

                itemsWithNoValue.AddRange(Take(pendingItem, pendingItem.AvailableQuantity));
            }

            var items = charge.PosChargeSelectedMenuItems!.Any() == true ? GetPayingItemsFromUsersSelection(availableItems, availableItemsDictionary, charge) : GetPayingItemsFromFreePayment(availableItems, charge);
            return itemsWithNoValue.Concat(items);
        }

        private IEnumerable<(OrderMenuItem ItemPaid, decimal Quantity)> Take(GrouppedOrderMenuItem group, decimal totalQuantityToTake)
        {
            var remainingQuantityToTake = totalQuantityToTake;
            foreach (var item in group.Items)
            {
                if (item.AvailableQuantity <= 0)
                    continue;

                var quantityToTake = remainingQuantityToTake;
                if (item.AvailableQuantity < quantityToTake)
                    quantityToTake = item.AvailableQuantity;

                yield return (item.OrderMenuItem, quantityToTake);

                remainingQuantityToTake -= quantityToTake;
                if (Math.Abs(remainingQuantityToTake) <= quantityMargin)
                    break;
            }
        }

        private IEnumerable<(OrderMenuItem ItemPaid, decimal Quantity)> GetPayingItemsFromUsersSelection(IEnumerable<GrouppedOrderMenuItem> availableItems, IReadOnlyDictionary<int, GrouppedOrderMenuItem> availableItemsDictionary, PosCharge posCharge)
        {
            Dictionary<GrouppedOrderMenuItem, decimal> availableQuantities = availableItemsDictionary.Values.Distinct().ToDictionary(g => g, g => g.AvailableQuantity);
            List<(OrderMenuItem, decimal)> result = [];
            foreach (var selectedItem in posCharge.PosChargeSelectedMenuItems!)
            {
                decimal quantity = selectedItem.Quantity;

                if (availableItemsDictionary.TryGetValue(selectedItem.OrderMenuItemId, out var orderedItem))
                {
                    decimal availableQuantity = availableQuantities[orderedItem];
                    if (availableQuantity <= 0)
                        return GetPayingItemsFromFreePayment(availableItems, posCharge);

                    decimal quantityToTake = quantity - availableQuantity >= 0 ? availableQuantity : quantity;
                    quantity -= quantityToTake;
                    result.AddRange(Take(orderedItem, quantityToTake));
                    availableQuantities[orderedItem] = availableQuantity - quantityToTake;
                }

                if (Math.Abs(quantity) > quantityMargin)
                    return GetPayingItemsFromFreePayment(availableItems, posCharge);
            }
            return result;
        }

        private IEnumerable<(OrderMenuItem ItemPaid, decimal Quantity)> GetPayingItemsFromFreePayment(IEnumerable<GrouppedOrderMenuItem> availableItems, PosCharge posCharge)
        {
            List<(OrderMenuItem, decimal)> result = [];
            decimal amountToTakeRemaining = posCharge.Payment;
            foreach (var item in availableItems)
            {
                if (item.AvailableQuantity <= 0)
                    continue;

                decimal itemUnitPrice = item.SessionItem.GetUnitPrice(maxDecimalPlaces);
                decimal quantityToTake = itemUnitPrice * item.AvailableQuantity <= amountToTakeRemaining ? item.AvailableQuantity : amountToTakeRemaining / itemUnitPrice;
                result.AddRange(Take(item, quantityToTake));

                decimal amountTaken = quantityToTake * itemUnitPrice;
                amountToTakeRemaining -= amountTaken;
                if (Math.Abs(amountToTakeRemaining) <= quantityMargin)
                    return result;
            }
            return result;
        }

        protected bool HasPendingItems(Session session)
        {
            var sessionItems = session.GetValidOrderMenuItems().AsPaidSessionItems();
            foreach (var model in sessionItems)
            {
                var unpaidQuantity = model.Quantity - model.PaidQuantity;
                if (unpaidQuantity > 0)
                    return true;
            }
            return false;
        }

        private void ProcessInvoice(AQuiviSyncStrategy syncStrategy, PosCharge posCharge, decimal paymentAmount, IEnumerable<(OrderMenuItem, decimal)> itemsToBePaid)
        {
            var itemsToBePaidData = itemsToBePaid.SelectMany(e => (e.Item1.Modifiers ?? []).Select(m =>
                                                                                            {
                                                                                                var extraQuantityPerParent = e.Item1.Quantity == 0 ? 0 : m.Quantity / e.Item1.Quantity;
                                                                                                return (m, e.Item2 * extraQuantityPerParent);
                                                                                            }).Prepend(e)
                                                            ).Select(i => new
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

            var posChargeId = posCharge.Id;
            backgroundJobHandler.Enqueue(() => syncStrategy.ProcessInvoiceJob(posChargeId, paymentAmount, items));
        }
    }
}