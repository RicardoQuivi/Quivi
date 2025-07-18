using Quivi.Application.Extensions.Pos;
using Quivi.Application.Pos;
using Quivi.Application.Pos.Items;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Orders;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Events.Data.Sessions;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos.Commands;
using Quivi.Infrastructure.Abstractions.Pos.Exceptions;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Commands.Pos
{
    public class ProcessQuiviOrderCancelationAsyncCommand : ICommand<Task<IEnumerable<IEvent>>>
    {
        public int OrderId { get; set; }
        public string? Reason { get; set; }
    }

    public class ProcessQuiviOrderAsyncCommand : ICommand<Task<IEnumerable<IEvent>>>
    {
        public required IEnumerable<int> OrderIds { get; set; }
        public OrderState FromState { get; set; }
        public bool ToFinalState { get; set; }
        public required AQuiviSyncStrategy SyncStrategy { get; init; }
    }

    public class ProcessQuiviOrderAsyncCommandHandler : ASyncCommandHandler<ProcessQuiviOrderAsyncCommand>,
                                                            ICommandHandler<ProcessQuiviOrderAsyncCommand, Task<IEnumerable<IEvent>>>,
                                                            ICommandHandler<ProcessQuiviOrderCancelationAsyncCommand, Task<IEnumerable<IEvent>>>
    {
        private class ExtendedOrderMenuItem
        {
            public required OrderMenuItem OrderMenuItem { get; init; }
            public decimal PaidQuantity { get; init; }
            public decimal AvailableQuantity => OrderMenuItem.Quantity - PaidQuantity;
            public required SessionItem SessionItem { get; init; }
        }

        private enum ProcessingType
        {
            Next,
            Complete,
            Cancel
        }

        private class OrderData
        {
            public Order Order { get; }
            public OrderState OriginalState { get; }
            public OrderState NextState { get; }

            public OrderData(Order order, ProcessingType processingType)
            {
                Order = order;
                OriginalState = Order.State;
                NextState = GetNextState(Order, OriginalState, processingType);
                if (NextState == OriginalState)
                    throw new PosOrderAlreadySyncedException(Order.Id, OriginalState, NextState);
            }

            private OrderState GetNextState(Order order, OrderState state, ProcessingType processingType = ProcessingType.Next)
            {
                switch (processingType)
                {
                    case ProcessingType.Next:
                        switch (state)
                        {
                            case OrderState.Draft:
                                if (order.ScheduledTo.HasValue)
                                    return OrderState.ScheduledRequested;
                                if (order.PayLater && order.Channel!.ChannelProfile!.Features.HasFlag(ChannelFeature.PostPaidOrderingAutoApproval))
                                    return GetNextState(order, OrderState.Requested);
                                else if (order.PayLater == false && order.Channel!.ChannelProfile!.Features.HasFlag(ChannelFeature.PrePaidOrderingAutoApproval))
                                    return GetNextState(order, OrderState.Requested);
                                return OrderState.Requested;

                            case OrderState.Requested:
                                if (order.PayLater && order.Channel!.ChannelProfile!.Features.HasFlag(ChannelFeature.PostPaidOrderingAutoComplete))
                                    return GetNextState(order, OrderState.Processing);
                                else if (order.PayLater == false && order.Channel!.ChannelProfile!.Features.HasFlag(ChannelFeature.PrePaidOrderingAutoComplete))
                                    return GetNextState(order, OrderState.Processing);
                                return OrderState.Processing;

                            case OrderState.Rejected: return OrderState.Rejected;
                            case OrderState.Processing: return OrderState.Completed;
                            case OrderState.Completed: return OrderState.Completed;

                            case OrderState.ScheduledRequested: return OrderState.Scheduled;
                            case OrderState.Scheduled: return GetNextState(order, OrderState.Requested);
                        }
                        break;
                    case ProcessingType.Complete: return OrderState.Completed;
                    case ProcessingType.Cancel:
                        if (state == OrderState.Completed)
                            throw new Exception("Order is already completed!");
                        return OrderState.Rejected;
                }

                throw new NotImplementedException();
            }

        }

        private readonly IUnitOfWork unitOfWork;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IBackgroundJobHandler backgroundJobHandler;

        private readonly ISessionsRepository sessionsRepo;
        private readonly IPosChargesRepository posChargesRepo;
        private readonly IOrdersRepository ordersRepo;
        //private readonly IOrderConfigurableFieldsRepository configurableFieldsRepo;

        public ProcessQuiviOrderAsyncCommandHandler(IUnitOfWork unitOfWork,
                                                        IDateTimeProvider dateTimeProvider,
                                                        IBackgroundJobHandler backgroundJobHandler)
        {
            this.dateTimeProvider = dateTimeProvider;
            this.backgroundJobHandler = backgroundJobHandler;

            this.unitOfWork = unitOfWork;
            this.sessionsRepo = unitOfWork.Sessions;
            this.ordersRepo = unitOfWork.Orders;
            this.posChargesRepo = unitOfWork.PosCharges;
            //configurableFieldsRepo = unitOfWork.OrderConfigurableFieldRepository;
        }

        public async Task<IEnumerable<IEvent>> Handle(ProcessQuiviOrderCancelationAsyncCommand command)
        {
            var orderDatas = await Initialize([command.OrderId], ProcessingType.Cancel);
            if (orderDatas.Any() == false)
                return [];
            return await CancelOrder(orderDatas.Single(), command.Reason);
        }

        protected override async Task Sync(ProcessQuiviOrderAsyncCommand command)
        { 
            var orderDatas = await Initialize(command.OrderIds, command.ToFinalState ? ProcessingType.Complete : ProcessingType.Next, command.FromState);
            if (orderDatas.Any() == false)
                return;

            foreach (var orderData in orderDatas)
                await ProcessToNextState(command, orderData);
        }

        private async Task<IEnumerable<OrderData>> Initialize(IEnumerable<int> orderIds, ProcessingType processingType, OrderState? fromState = null)
        {
            var orders = await ordersRepo.GetAsync(new GetOrdersCriteria
            {
                Ids = orderIds,
                IncludeChangeLogs = true,
                IncludeOrderMenuItems = true,
                IncludeOrderMenuItemsAndMofifiers = true,
                IncludeOrderMenuItemsPosChargeInvoiceItems = true,
                IncludeChannelProfile = true,

                PageIndex = 0,
                PageSize = null,
            });

            List<OrderData> result = new List<OrderData>();
            foreach (var order in orders)
            {
                if (fromState.HasValue && order.State != fromState.Value)
                    continue;

                try
                {
                    result.Add(new OrderData(order, processingType));
                }
                catch (PosOrderAlreadySyncedException) { }
            }
            return result;
        }

        private async Task<IEnumerable<IEvent>> CancelOrder(OrderData orderData, string? reason)
        {
            var order = orderData.Order;
            order.State = OrderState.Rejected;
            order.ModifiedDate = dateTimeProvider.GetUtcNow();
            AddChangeLog(reason, orderData);
            await unitOfWork.SaveChangesAsync();
            return [
                new OnOrderOperationEvent
                {
                    Operation = EntityOperation.Update,
                    MerchantId = order.MerchantId,
                    Id = order.Id,
                    ChannelId = order.ChannelId,
                },
            ];
        }

        private void AddChangeLog(string? reason, OrderData orderData)
        {
            var order = orderData.Order;
            order.OrderChangeLogs = new List<OrderChangeLog>(order.OrderChangeLogs!)
            {
                new OrderChangeLog
                {
                    State = orderData.NextState,
                    OrderId = order.Id,
                    Notes = reason,
                    CreatedDate = order.ModifiedDate,
                    ModifiedDate = order.ModifiedDate,
                    Order = order,
                }
            };
        }

        private async Task ProcessToNextState(ProcessQuiviOrderAsyncCommand command, OrderData orderData)
        {
            var order = orderData.Order;
            if (order.PayLater)
                order.OrderType = OrderType.OnSite;

            AddChangeLog(null, orderData);
            order.State = orderData.NextState;
            order.ModifiedDate = dateTimeProvider.GetUtcNow();

            AddOrderEvent(order, o => new OnOrderOperationEvent
            {
                Operation = EntityOperation.Update,
                MerchantId = o.MerchantId,
                Id = o.Id,
                ChannelId = o.ChannelId,
            });

            await ProcessOrderToSession(command.SyncStrategy, orderData);
            await unitOfWork.SaveChangesAsync();

            if (order.State == OrderState.Rejected)
                return;

            if (new[] { OrderState.Draft, OrderState.Requested, OrderState.ScheduledRequested }.Contains(orderData.OriginalState) &&
                    new[] { OrderState.Processing, OrderState.Completed, OrderState.Scheduled }.Contains(orderData.NextState))
                AddOrderEvent(order, o => new OnOrderCommitedEvent
                {
                    ChannelId = o.ChannelId,
                    SessionId = o.SessionId!.Value,
                    Id = o.Id,
                    MerchantId = o.MerchantId,
                });

            if (orderData.NextState == OrderState.Scheduled)
                AddOrderEvent(order, o => new OnOrderScheduledEvent
                {
                    ChannelId = o.ChannelId,
                    Id = o.Id,
                    MerchantId = o.MerchantId,
                });

            if (order.State == OrderState.Completed)
                AddOrderEvent(order, o => new OnOrderCompletedEvent
                {
                    ChannelId = o.ChannelId,
                    Id = o.Id,
                    MerchantId = o.MerchantId,
                });

            if(order.Origin == OrderOrigin.GuestsApp && order.State == OrderState.Requested)
                AddOrderEvent(order, o => new OnOrderPendingApprovalEvent
                {
                    ChannelId = o.ChannelId,
                    Id = o.Id,
                    MerchantId = o.MerchantId,
                });
        }

        private async Task ProcessOrderToSession(AQuiviSyncStrategy syncStrategy, OrderData orderData)
        {
            var order = orderData.Order;
            if (order.SessionId.HasValue || new[] { OrderState.ScheduledRequested, OrderState.Requested }.Contains(orderData.NextState))
                return;

            if (order.PayLater && order.Channel!.ChannelProfile!.Features.HasFlag(ChannelFeature.AllowsSessions) == false)
            {
                order.State = OrderState.Rejected;
                var orderChange = order.OrderChangeLogs!.Last();
                orderChange.Notes = "System rejected this order";
                orderChange.State = OrderState.Rejected;
                return;
            }

            var items = order.OrderMenuItems!.AsSessionItems();
            if(items.Any() == false)
            {
                order.State = OrderState.Completed;
                return;
            }

            await AssignToSession(syncStrategy, order);
        }

        private async Task AssignToSession(AQuiviSyncStrategy syncStrategy, Order order)
        {
            var now = dateTimeProvider.GetUtcNow();
            Session? session = await GetOrCreateSession(order);
            if (session == null)
            {
                session = new Session
                {
                    Status = SessionStatus.Ordering,

                    EmployeeId = order.EmployeeId,
                    ChannelId = order.ChannelId,
                    PosIdentifier = null,
                    StartDate = now,
                    EndDate = null,

                    CreateDate = now,
                    ModifiedDate = now,

                    Orders = new List<Order>(),
                };
                sessionsRepo.Add(session);
                await OnSessionAdded(order, session);
            }
            else
                AddSessionEvent(session, s => new OnSessionOperationEvent
                {
                    Operation = EntityOperation.Update,
                    ChannelId = s.ChannelId,
                    Id = s.Id,
                    MerchantId = order.MerchantId,
                });

            session.Orders!.Add(order);
            order.Session = session;
            order.SessionId = session.Id;

            if (order.PayLater == false)
                await ProcessPrePaidOrder(syncStrategy, order, session);

            if (HasPendingItems(session) == false)
            {
                session.Status = SessionStatus.Closed;
                session.EndDate = now;
            }
        }

        private async Task ProcessPrePaidOrder(AQuiviSyncStrategy syncStrategy, Order order, Session session)
        {
            var posChargesQuery = await posChargesRepo.GetAsync(new GetPosChargesCriteria
            {
                MerchantIds = [order.MerchantId],
                ChannelIds = [order.ChannelId],
                OrderIds = [order.Id],
                HasSession = false,
                IsCaptured = true,
                IncludePosChargeSelectedMenuItems = true,
                PageSize = 1,
            });
            var posCharge = posChargesQuery.Single();

            posCharge.SessionId = session.Id;
            posCharge.Session = session;
            AddSessionEvent(session, s => new OnPosChargeOperationEvent
            {
                Operation = EntityOperation.Update,
                ChannelId = posCharge.ChannelId,
                MerchantId = posCharge.MerchantId,
                Id = posCharge.Id,
            });

            //TODO: Most of the code bellow is equal or very simiar to ProcessQuiviSyncChargeAsyncCommandHandler. Maybe a refactor would be a good idea
            decimal paymentAmount = ProcessPayment(syncStrategy, session, posCharge);
            posCharge.PosChargeSyncAttempts = new List<PosChargeSyncAttempt>
            {
                new PosChargeSyncAttempt
                {
                    PosCharge = posCharge,
                    PosChargeId = posCharge.Id,
                    SyncedAmount = paymentAmount,
                    State = SyncAttemptState.Synced,
                    CreatedDate = dateTimeProvider.GetUtcNow(),
                    ModifiedDate = dateTimeProvider.GetUtcNow(),
                },
            };
        }

        private decimal ProcessPayment(AQuiviSyncStrategy syncStrategy, Session session, PosCharge posCharge)
        {
            var itemsToBePaid = GetAndProcessPayingItems(posCharge, session);
            decimal paymentAmount = Math.Round(itemsToBePaid.Sum(p => p.Item.FinalPrice * p.Quantity), maxDecimalPlaces);

            if (HasPendingItems(session) == false)
            {
                session.Status = SessionStatus.Closed;
                session.EndDate = dateTimeProvider.GetUtcNow();
            }

            ProcessInvoice(syncStrategy, posCharge, paymentAmount, itemsToBePaid);
            return paymentAmount;
        }

        private void ProcessInvoice(AQuiviSyncStrategy syncStrategy, PosCharge posCharge, decimal paymentAmount, IEnumerable<(OrderMenuItem, decimal)> itemsToBePaid)
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
            backgroundJobHandler.Enqueue(() => syncStrategy.ProcessInvoiceJob(posChargeId, paymentAmount, items));
        }

        private async Task<Session?> GetOrCreateSession(Order order)
        {
            if (order.Channel!.ChannelProfile!.Features.HasFlag(ChannelFeature.AllowsSessions) == false)
                return null;

            var sessionsQuery = await sessionsRepo.GetAsync(new GetSessionsCriteria
            {
                MerchantIds = [order.MerchantId],
                ChannelIds = [order.ChannelId],
                Statuses = [SessionStatus.Ordering],

                IncludeOrdersMenuItems = true,
                IncludeOrdersMenuItemsPosChargeInvoiceItems = true,
                IncludeOrdersMenuItemsModifiers = true,
                PageIndex = 0,
                PageSize = 1,
            });

            var session = sessionsQuery.SingleOrDefault();
            return session;
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
                    var extra = new PosChargeInvoiceItem
                    {
                        PosCharge = posCharge,
                        Quantity = payingExtra.Quantity,
                        OrderMenuItemId = payingExtra.Id,
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
                                                    PaidQuantity = omi.PosChargeInvoiceItems?.Sum(ii => ii.Quantity) ?? 0.0m,
                                                    SessionItem = omi.AsSessionItem(),
                                                })
                                                .GroupBy(e => e.SessionItem, comparer)
                                                .Select(g => new
                                                {
                                                    TotalPaidQuantity = g.Sum(s => s.PaidQuantity),
                                                    TotalQuantity = g.Sum(s => s.OrderMenuItem.Quantity),
                                                    Items = g.AsEnumerable(),
                                                })
                                                .Where(e => e.TotalQuantity > e.TotalPaidQuantity)
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


        protected Task OnSessionAdded(Order order, Session session)
        {
            AddOrderEvent(order, o => new OnSessionOperationEvent
            {
                Operation = EntityOperation.Create,
                ChannelId = o.ChannelId,
                Id = o.SessionId!.Value,
                MerchantId = o.MerchantId,
            });

            return Task.CompletedTask;
            //TODO: ConfigurableFields

            //var configurableFields = await configurableFieldsRepo.GetAsync(new GetOrderConfigurableFieldsCriteria
            //{
            //    ChannelIds = [session.ChannelId],
            //    ForPoSSessions = true,
            //    IsAutoFill = true,
            //    IsDeleted = false,
            //});

            //session.SessionAdditionalInfos = configurableFields.Select(c => new SessionAdditionalInfo
            //{
            //    OrderConfigurableFieldId = c.OrderConfigurableFieldId,
            //    Value = c.DefaultValue,
            //}).ToList();
        }
    }
}
