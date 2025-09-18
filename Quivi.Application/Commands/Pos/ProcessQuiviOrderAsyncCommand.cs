using Quivi.Application.Extensions.Pos;
using Quivi.Application.Pos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Orders;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Events.Data.Sessions;
using Quivi.Infrastructure.Abstractions.Jobs;
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
        public required AQuiviSyncStrategy SyncStrategy { get; init; }
    }

    public class ProcessQuiviOrderAsyncCommandHandler : AProcessChargeAsyncCommandHandler<ProcessQuiviOrderAsyncCommand>,
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

            private OrderState GetNextState(Order order, OrderState fromState, ProcessingType processingType = ProcessingType.Next)
            {
                switch (processingType)
                {
                    case ProcessingType.Next:
                        switch (fromState)
                        {
                            case OrderState.Draft:
                                {
                                    if (order.ScheduledTo.HasValue)
                                        return OrderState.ScheduledRequested;

                                    if (order.PayLater && order.Channel!.ChannelProfile!.Features.HasFlag(ChannelFeature.PostPaidOrderingAutoApproval))
                                        return GetNextState(order, OrderState.Accepted);

                                    if (order.PayLater == false && order.Channel!.ChannelProfile!.Features.HasFlag(ChannelFeature.PrePaidOrderingAutoApproval))
                                        return GetNextState(order, OrderState.Accepted);

                                    return OrderState.PendingApproval;
                                }
                            case OrderState.PendingApproval:
                            case OrderState.Accepted:
                                {
                                    if (order.PayLater && order.Channel!.ChannelProfile!.Features.HasFlag(ChannelFeature.PostPaidOrderingAutoComplete))
                                        return GetNextState(order, OrderState.Processing);

                                    if (order.PayLater == false && order.Channel!.ChannelProfile!.Features.HasFlag(ChannelFeature.PrePaidOrderingAutoComplete))
                                        return GetNextState(order, OrderState.Processing);

                                    return OrderState.Processing;
                                }
                            case OrderState.Rejected: return OrderState.Rejected;
                            case OrderState.Processing: return OrderState.Completed;
                            case OrderState.Completed: return OrderState.Completed;

                            case OrderState.ScheduledRequested: return OrderState.Scheduled;
                            case OrderState.Scheduled: return GetNextState(order, OrderState.Accepted);
                        }
                        break;
                    case ProcessingType.Complete: return OrderState.Completed;
                    case ProcessingType.Cancel:
                        if (fromState == OrderState.Completed)
                            throw new Exception("Order is already completed!");
                        return OrderState.Rejected;
                }

                throw new NotImplementedException();
            }

        }

        private readonly IUnitOfWork unitOfWork;

        private readonly ISessionsRepository sessionsRepo;
        private readonly IPosChargesRepository posChargesRepo;
        private readonly IOrdersRepository ordersRepo;
        private readonly IOrderSequencesRepository sequencesRepository;
        private readonly IOrderConfigurableFieldsRepository configurableFieldsRepo;

        public ProcessQuiviOrderAsyncCommandHandler(IUnitOfWork unitOfWork,
                                                        IDateTimeProvider dateTimeProvider,
                                                        IBackgroundJobHandler backgroundJobHandler) : base(dateTimeProvider, backgroundJobHandler)
        {
            this.unitOfWork = unitOfWork;
            this.sessionsRepo = unitOfWork.Sessions;
            this.ordersRepo = unitOfWork.Orders;
            this.posChargesRepo = unitOfWork.PosCharges;
            this.sequencesRepository = unitOfWork.OrderSequences;
            this.configurableFieldsRepo = unitOfWork.OrderConfigurableFields;
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
                IncludeOrderAdditionalFields = true,
                IncludeChannelProfile = true,
                IncludeOrderSequence = true,

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
            var now = dateTimeProvider.GetUtcNow();
            order.State = orderData.NextState;
            order.ModifiedDate = now;
            if (order.OrderSequence == null)
            {
                var lastOrderOfMerchantQuery = await sequencesRepository.GetAsync(new GetOrderSequencesCriteria
                {
                    MerchantIds = [order.MerchantId],
                    PageSize = 1,
                });

                order.OrderSequence = new OrderSequence
                {
                    SequenceNumber = (lastOrderOfMerchantQuery.SingleOrDefault()?.SequenceNumber ?? 0) + 1,
                    CreatedDate = now,
                    ModifiedDate = now,
                };
            }

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

            if (new[] { OrderState.Draft, OrderState.PendingApproval, OrderState.Accepted, OrderState.ScheduledRequested }.Contains(orderData.OriginalState) &&
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

            if (order.State == OrderState.PendingApproval)
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
            if (order.SessionId.HasValue || new[] { OrderState.ScheduledRequested, OrderState.Accepted, OrderState.PendingApproval }.Contains(orderData.NextState))
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
            if (items.Any() == false)
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

            decimal paymentAmount = await base.ProcessPayment(syncStrategy, session, posCharge, () => Task.CompletedTask);
            posCharge.PosChargeSyncAttempts = new List<PosChargeSyncAttempt>
            {
                new PosChargeSyncAttempt
                {
                    PosCharge = posCharge,
                    PosChargeId = posCharge.Id,
                    SyncedAmount = paymentAmount,
                    State = SyncAttemptState.Synced,
                    Type = SyncAttemptType.Payment,
                    CreatedDate = dateTimeProvider.GetUtcNow(),
                    ModifiedDate = dateTimeProvider.GetUtcNow(),
                },
            };
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

        private async Task OnSessionAdded(Order order, Session session)
        {
            AddOrderEvent(order, o => new OnSessionOperationEvent
            {
                Operation = EntityOperation.Create,
                ChannelId = o.ChannelId,
                Id = o.SessionId!.Value,
                MerchantId = o.MerchantId,
            });

            var configurableFields = await configurableFieldsRepo.GetAsync(new GetOrderConfigurableFieldsCriteria
            {
                ChannelIds = [session.ChannelId],
                ForPosSessions = true,
                IsAutoFill = true,
                IsDeleted = false,
            });

            if (configurableFields.Any() == false)
                return;


            order.OrderAdditionalInfos = configurableFields.Where(c => string.IsNullOrWhiteSpace(c.DefaultValue) == false).Select(c => new OrderAdditionalInfo
            {
                OrderConfigurableFieldId = c.Id,
                Value = c.DefaultValue!,
            }).ToList();
        }
    }
}
