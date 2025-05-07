using Quivi.Application.Extensions.Pos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Orders;
using Quivi.Infrastructure.Abstractions.Events.Data.Sessions;
using Quivi.Infrastructure.Abstractions.Pos.Commands;
using Quivi.Infrastructure.Abstractions.Pos.Exceptions;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

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
    }

    public class ProcessQuiviOrderAsyncCommandHandler : ASyncCommandHandler<ProcessQuiviOrderAsyncCommand>,
                                                            ICommandHandler<ProcessQuiviOrderAsyncCommand, Task<IEnumerable<IEvent>>>,
                                                            ICommandHandler<ProcessQuiviOrderCancelationAsyncCommand, Task<IEnumerable<IEvent>>>
    {
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

        private readonly ISessionsRepository sessionsRepo;
        private readonly IOrdersRepository ordersRepo;
        //private readonly IOrderConfigurableFieldsRepository configurableFieldsRepo;

        public ProcessQuiviOrderAsyncCommandHandler(IUnitOfWork unitOfWork,
                                                        IDateTimeProvider dateTimeProvider)
        {
            this.dateTimeProvider = dateTimeProvider;
            this.unitOfWork = unitOfWork;
            this.sessionsRepo = unitOfWork.Sessions;
            this.ordersRepo = unitOfWork.Orders;
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
                }
            ];
        }

        private void AddChangeLog(string? reason, OrderData orderData)
        {
            var order = orderData.Order;
            order.OrderChangeLogs = new List<OrderChangeLog>(order.OrderChangeLogs!)
            {
                new OrderChangeLog
                {
                    EatOrderState = orderData.NextState,
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

            await ProcessOrderToSession(orderData);
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
        }

        private async Task ProcessOrderToSession(OrderData orderData)
        {
            var order = orderData.Order;
            if (order.SessionId.HasValue || new[] { OrderState.ScheduledRequested, OrderState.Requested }.Contains(orderData.NextState))
            {
                await unitOfWork.SaveChangesAsync();
                return;
            }

            if (order.Channel!.ChannelProfile!.Features.HasFlag(ChannelFeature.AllowsSessions))
            {
                var items = order.OrderMenuItems!.AsSessionItems();
                if(items.Any() == false)
                {
                    order.State = OrderState.Completed;
                    await unitOfWork.SaveChangesAsync();
                    return;
                }
                await AssignToSession(order);
                await unitOfWork.SaveChangesAsync();
                return;
            }

            if (order.PayLater)
            {
                order.State = OrderState.Rejected;
                var orderChange = order.OrderChangeLogs!.Last();
                orderChange.Notes = "System rejected this order";
                orderChange.EatOrderState = OrderState.Rejected;
                await unitOfWork.SaveChangesAsync();
                return;
            }

            //Creates a Completed Session to make sure all orders generates a session
            order.Session = new Session
            {
                CreateDate = order.CreatedDate,
                ChannelId = order.ChannelId,
                Status = SessionStatus.Closed,
                StartDate = order.CreatedDate,
                EndDate = dateTimeProvider.GetUtcNow(),
                Orders = [order],
                EmployeeId = order.EmployeeId,
            };
            await OnSessionAdded(order.Session);

            await unitOfWork.SaveChangesAsync();
            AddOrderEvent(order, o => new OnSessionOperationEvent
            {
                Operation = EntityOperation.Create,
                ChannelId = o.ChannelId,
                Id = o.SessionId!.Value,
                MerchantId = o.MerchantId,
            });
        }

        private async Task AssignToSession(Order order)
        {
            var sessionsQuery = await sessionsRepo.GetAsync(new GetSessionsCriteria
            {
                MerchantIds = [order.MerchantId],
                ChannelIds = [order.ChannelId],
                Statuses = [SessionStatus.Ordering],

                IncludeOrdersMenuItems = true,
                IncludeOrdersMenuItemsPosChargeInvoiceItems = true,
                PageIndex = 0,
                PageSize = 1,
            });

            var session = sessionsQuery.SingleOrDefault();
            if(session == null)
            {
                session = new Session
                {
                    Status = SessionStatus.Ordering,

                    EmployeeId = order.EmployeeId,
                    ChannelId = order.ChannelId,
                    PosIdentifier = null,
                    StartDate = dateTimeProvider.GetUtcNow(),
                    EndDate = null,

                    CreateDate = dateTimeProvider.GetUtcNow(),
                    ModifiedDate = dateTimeProvider.GetUtcNow(),

                    Orders = new List<Order>(),
                };
                sessionsRepo.Add(session);
                AddOrderEvent(order, o => new OnSessionOperationEvent
                {
                    Operation = EntityOperation.Create,
                    ChannelId = o.ChannelId,
                    Id = o.SessionId!.Value,
                    MerchantId = o.MerchantId,
                });
                await OnSessionAdded(session);
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

            if(HasPendingItems(session) == false)
                session.Status = SessionStatus.Closed;
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

        protected Task OnSessionAdded(Session session)
        {
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
