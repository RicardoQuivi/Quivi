using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Orders;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Orders
{
    public interface IUpdatableOrderField : IUpdatableEntity
    {
        string Value { get; set; }
    }

    public interface IUpdatableOrderFields : IUpdatableEntity
    {
        IUpdatableOrderField this[int id] { get; }
        bool ContainsKey(int id);
        bool Remove(int id);
        void Clear();
        void Upsert(int id, Action<IUpdatableOrderField> t);
    }

    public interface IUpdatableOrder : IUpdatableEntity
    {
        int MerchantId { get; }
        int ChannelId { get; }
        DateTime CreatedDate { get; }
        bool ShouldAutoComplete { get; }

        DateTime? ScheduledTo { get; set; }
        bool PayLater { get; set; }
        OrderState State { get; set; }

        IUpdatableOrderFields Fields { get; }
    }

    public class UpdateOrdersAsyncCommand : AUpdateAsyncCommand<IEnumerable<Order>, IUpdatableOrder>
    {
        public required GetOrdersCriteria Criteria { get; init; }
    }

    public class UpdateOrderAsyncCommandHandler : ICommandHandler<UpdateOrdersAsyncCommand, Task<IEnumerable<Order>>>
    {
        private readonly IOrdersRepository repository;
        private readonly IEventService eventService;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IBackgroundJobHandler backgroundJobHandler;

        public UpdateOrderAsyncCommandHandler(IOrdersRepository repository,
                                                    IEventService eventService,
                                                    IDateTimeProvider dateTimeProvider,
                                                    IBackgroundJobHandler backgroundJobHandler)
        {
            this.repository = repository;
            this.eventService = eventService;
            this.dateTimeProvider = dateTimeProvider;
            this.backgroundJobHandler = backgroundJobHandler;
        }

        private class UpdatableOrder : IUpdatableOrder, IUpdatableOrderFields
        {
            private class UpdatableOrderField : IUpdatableOrderField
            {
                public readonly OrderAdditionalInfo Model;
                private readonly bool isNew;
                private readonly string originalValue;

                public UpdatableOrderField(OrderAdditionalInfo model, bool isNew)
                {
                    Model = model;
                    this.isNew = isNew;
                    originalValue = model.Value;
                }

                public string Value { get => Model.Value; set => Model.Value = value; }

                public bool HasChanges => isNew || originalValue != Model.Value;
            }

            public readonly Order Model;
            public DateTime? OriginalScheduledTo { get; }
            public OrderState OriginalState { get; }
            public bool OriginalPayLater { get; }

            //TODO: This should be dynamic according to current state and Order type
            private IEnumerable<(OrderState From, OrderState To)> AllowedTransitions
            {
                get
                {
                    yield return (OrderState.Draft, OrderState.ScheduledRequested);
                    yield return (OrderState.Draft, OrderState.Rejected);
                    yield return (OrderState.Draft, OrderState.Accepted);
                    yield return (OrderState.Draft, OrderState.Processing);
                    yield return (OrderState.Draft, OrderState.Completed);

                    #region When order has a Schedule
                    yield return (OrderState.ScheduledRequested, OrderState.Scheduled);
                    yield return (OrderState.ScheduledRequested, OrderState.Rejected);

                    yield return (OrderState.Scheduled, OrderState.Processing);
                    yield return (OrderState.Scheduled, OrderState.Accepted);
                    yield return (OrderState.Scheduled, OrderState.Rejected);
                    #endregion

                    yield return (OrderState.PendingApproval, OrderState.Accepted);
                    yield return (OrderState.PendingApproval, OrderState.Processing);
                    yield return (OrderState.PendingApproval, OrderState.Completed);
                    yield return (OrderState.PendingApproval, OrderState.Rejected);

                    yield return (OrderState.Accepted, OrderState.Processing);
                    yield return (OrderState.Accepted, OrderState.Completed);
                    yield return (OrderState.Accepted, OrderState.Rejected);

                    yield return (OrderState.Scheduled, OrderState.Processing);
                    yield return (OrderState.Scheduled, OrderState.Rejected);

                    yield return (OrderState.Processing, OrderState.Completed);
                    yield return (OrderState.Processing, OrderState.Rejected);
                }
            }

            #region IUpdatableOrderFields
            private readonly ICollection<OrderAdditionalInfo> _additionalFields;
            private readonly HashSet<OrderAdditionalInfo> _originalAdditionalFields;
            private readonly Dictionary<int, (OrderAdditionalInfo Model, UpdatableOrderField Value)> _fieldsDictionary;
            #endregion

            public UpdatableOrder(Order model)
            {
                this.Model = model;
                OriginalScheduledTo = this.Model.ScheduledTo;
                OriginalState = this.Model.State;
                OriginalPayLater = this.Model.PayLater;

                _additionalFields = model.OrderAdditionalInfos ?? [];
                _fieldsDictionary = model.OrderAdditionalInfos?.ToDictionary(t => t.OrderConfigurableFieldId, t => (t, new UpdatableOrderField(t, false))) ?? [];
                _originalAdditionalFields = model.OrderAdditionalInfos?.ToHashSet() ?? [];
            }

            public int MerchantId => Model.MerchantId;
            public int ChannelId => Model.ChannelId;
            public DateTime CreatedDate => Model.CreatedDate;
            public bool ShouldAutoComplete => Model.Channel!.ChannelProfile!.Features.HasFlag(Model.PayLater ? ChannelFeature.PostPaidOrderingAutoComplete : ChannelFeature.PrePaidOrderingAutoComplete);

            public DateTime? ScheduledTo
            {
                get => Model.ScheduledTo;
                set
                {
                    if (OriginalState != OrderState.Draft)
                        throw new InvalidOperationException($"Can't change {nameof(Order.ScheduledTo)} when the state is not {OrderState.Draft}.");
                    Model.ScheduledTo = value;
                }
            }

            public bool PayLater 
            {
                get => Model.PayLater;
                set
                {
                    if (OriginalState != OrderState.Draft)
                        throw new InvalidOperationException($"Can't change {nameof(Order.PayLater)} when the state is not {OrderState.Draft}.");
                    Model.PayLater = value;
                }
            }

            public OrderState State 
            {
                get => Model.State;
                set
                {
                    if (AllowedTransitions.Any(r => r.From == OriginalState && r.To == value) == false)
                        throw new InvalidOperationException($"Can't change {nameof(Order.State)} from {OriginalState} to {value}.");
                    Model.State = value;
                }
            }

            public bool HasChanges
            {
                get
                {
                    if (OriginalScheduledTo != Model.ScheduledTo)
                        return true;

                    if (OriginalState != Model.State)
                        return true;

                    if (OriginalPayLater != Model.PayLater)
                        return true;

                    if(IUpdatableOrderFieldsHasChanges)
                        return true;

                    return false;
                }
            }

            public IUpdatableOrderFields Fields => this;

            #region IUpdatableOrderFields
            public IUpdatableOrderField this[int id] => _fieldsDictionary[id].Value;

            public bool ContainsKey(int id) => _fieldsDictionary.ContainsKey(id);

            public bool Remove(int id)
            {
                if (_fieldsDictionary.TryGetValue(id, out var exists))
                    _additionalFields.Remove(exists.Model);
                return _fieldsDictionary.Remove(id);
            }

            public void Clear()
            {
                _fieldsDictionary.Clear();
                _additionalFields.Clear();
            }

            public void Upsert(int id, Action<IUpdatableOrderField> t)
            {
                if (_fieldsDictionary.TryGetValue(id, out var exists))
                {
                    t(exists.Value);
                    return;
                }

                var newEntry = new OrderAdditionalInfo
                {
                    Value = string.Empty,
                    OrderId = Model.Id,
                    OrderConfigurableFieldId = id
                };
                var entry = (newEntry, new UpdatableOrderField(newEntry, true));
                _additionalFields.Add(entry.newEntry);
                _fieldsDictionary[id] = entry;
                t(entry.Item2);
            }

            bool IUpdatableOrderFieldsHasChanges
            {
                get
                {
                    if (_fieldsDictionary.Count != _originalAdditionalFields.Count)
                        return true;

                    foreach (var entry in _fieldsDictionary.Values)
                    {
                        if (entry.Value.HasChanges)
                            return true;

                        if (_originalAdditionalFields.Contains(entry.Model) == false)
                            return true;
                    }
                    return false;
                }
            }
            #endregion
        }

        public async Task<IEnumerable<Order>> Handle(UpdateOrdersAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeChannel = true,
                IncludeChannelProfile = true,
                IncludeOrderMenuItems = true,
                IncludeOrderAdditionalFields = true,
                PageSize = 1,
            });
            if (entities.Any() == false)
                return entities;

            await Process(command.UpdateAction, entities);
            return entities;
        }

        private async Task<IEnumerable<Order>> Process(Func<IUpdatableOrder, Task> updateAction, IEnumerable<Order> entities)
        {
            var changedEntities = new List<UpdatableOrder>();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableOrder(entity);
                await updateAction.Invoke(updatableEntity);
                if (updatableEntity.HasChanges == false)
                    continue;

                entity.ModifiedDate = dateTimeProvider.GetUtcNow();
                changedEntities.Add(updatableEntity);
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();

            foreach (var updatableEntity in changedEntities)
            {
                var entity = updatableEntity.Model;

                if (updatableEntity.OriginalState != entity.State && entity.State == OrderState.Accepted)
                    await CreateOrderChangeEvent(entity);
                else
                    await GenerateOrderChangeEvent(entity);

                if (updatableEntity.OriginalState != entity.State && entity.State == OrderState.Scheduled && entity.ScheduledTo.HasValue)
                    ScheduledJob(entity.Id, entity.ScheduledTo.Value);
            }

            return entities;
        }

        private void ScheduledJob(int orderId, DateTime scheduledTo)
        {
            TimeSpan timeSpan = TimeSpan.FromMinutes(30);
            var shouldChangeAtDate = new DateTimeOffset(scheduledTo.Add(-timeSpan), TimeSpan.Zero);

            backgroundJobHandler.Schedule(() => MoveFromScheduledToRequested(orderId), shouldChangeAtDate);
        }

        public async Task MoveFromScheduledToRequested(int orderId)
        {
            var orderQuery = await repository.GetAsync(new GetOrdersCriteria
            {
                Ids = [orderId],
                States = [OrderState.Scheduled],
                PageSize = 1,
            });
            var entity = orderQuery.SingleOrDefault();

            if (entity == null) 
                return;

            await Process((IUpdatableOrder order) =>
            {
                if (order.State != OrderState.Scheduled)
                    return Task.CompletedTask;

                order.State = OrderState.Accepted;
                return Task.CompletedTask;
            }, [entity]);
        }

        private Task GenerateOrderChangeEvent(Order entity) => eventService.Publish(new OnOrderOperationEvent
        {
            Id = entity.Id,
            MerchantId = entity.MerchantId,
            ChannelId = entity.ChannelId,
            Operation = EntityOperation.Update,
        });

        private Task CreateOrderChangeEvent(Order entity) => eventService.Publish(new OnOrderCommitedEvent
        {
            Id = entity.Id,
            MerchantId = entity.MerchantId,
            ChannelId = entity.ChannelId,
            SessionId = entity.SessionId!.Value,
        });
    }
}
