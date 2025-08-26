using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.OrderAdditionalFields;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.OrderAdditionalInfos
{
    public interface IUpdatableOrderAdditionalInfo : IUpdatableEntity
    {
        public int OrderConfigurableFieldId { get; }
        public int OrderId { get; }
        public string Value { get; set; }
    }

    public class UpdateOrderAdditionalInfosAsyncCommand : AUpdateAsyncCommand<IEnumerable<OrderAdditionalInfo>, IUpdatableOrderAdditionalInfo>
    {
        public required GetOrderAdditionalInfosCriteria Criteria { get; init; }
    }

    public class UpdateOrderAdditionalInfosAsyncCommandHandler : ICommandHandler<UpdateOrderAdditionalInfosAsyncCommand, Task<IEnumerable<OrderAdditionalInfo>>>
    {
        private readonly IOrderAdditionalInfosRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateOrderAdditionalInfosAsyncCommandHandler(IOrderAdditionalInfosRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public class UpdatableOrderAdditionalInfo : IUpdatableOrderAdditionalInfo
        {
            public readonly OrderAdditionalInfo Model;
            private readonly string originalValue;
            public UpdatableOrderAdditionalInfo(OrderAdditionalInfo model)
            {
                this.Model = model;
                this.originalValue = model.Value;
            }

            public int OrderConfigurableFieldId => this.Model.OrderConfigurableFieldId;
            public int OrderId => this.Model.OrderId;

            public string Value { get => this.Model.Value; set => this.Model.Value = value; }

            public bool HasChanges => originalValue != this.Model.Value;
        }

        public async Task<IEnumerable<OrderAdditionalInfo>> Handle(UpdateOrderAdditionalInfosAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeOrderConfigurableField = true,
            });
            if (!entities.Any())
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            var changedEntities = new List<UpdatableOrderAdditionalInfo>();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableOrderAdditionalInfo(entity);
                await command.UpdateAction.Invoke(updatableEntity);

                if (updatableEntity.HasChanges)
                    changedEntities.Add(updatableEntity);
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();
            foreach (var entity in changedEntities)
                await eventService.Publish(new OnOrderAdditionalInfoOperationEvent
                {
                    MerchantId = entity.Model.OrderConfigurableField!.MerchantId,
                    OrderConfigurableFieldId = entity.OrderConfigurableFieldId,
                    OrderId = entity.OrderId,
                    Operation = EntityOperation.Update,
                });

            return entities;
        }
    }
}