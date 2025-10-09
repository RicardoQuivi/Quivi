using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroupChannelProfileAssociations;
using Quivi.Infrastructure.Abstractions.Events.Data.ChannelProfiles;
using Quivi.Infrastructure.Abstractions.Events.Data.OrderConfigurableFieldChannelProfileAssociations;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.ChannelProfiles
{
    public class UpdateChannelProfileAsyncCommand : AUpdateAsyncCommand<IEnumerable<ChannelProfile>, IUpdatableChannelProfile>
    {
        public required GetChannelProfilesCriteria Criteria { get; init; }
    }

    public class UpdateChannelProfileAsyncCommandHandler : ICommandHandler<UpdateChannelProfileAsyncCommand, Task<IEnumerable<ChannelProfile>>>
    {
        private readonly IChannelProfilesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateChannelProfileAsyncCommandHandler(IChannelProfilesRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<ChannelProfile>> Handle(UpdateChannelProfileAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeAssociatedOrderConfigurableFields = true,
            });
            if (!entities.Any())
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            var changedEntities = new List<UpdatableChannelProfile>();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableChannelProfile(entity, now);
                await command.UpdateAction.Invoke(updatableEntity);

                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;
                    changedEntities.Add(updatableEntity);
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();

            foreach (var entity in changedEntities)
            {
                await eventService.Publish(new OnChannelProfileOperationEvent
                {
                    Id = entity.Model.Id,
                    MerchantId = entity.Model.MerchantId,
                    Operation = EntityOperation.Update,
                });

                foreach (var changedEntity in entity.UpdatableOrderConfigurableFields.ChangedEntities)
                    await eventService.Publish(new OnOrderConfigurableFieldChannelProfileAssociationOperationEvent
                    {
                        MerchantId = entity.Model.MerchantId,
                        OrderConfigurableFieldId = changedEntity.Entity.OrderConfigurableFieldId,
                        ChannelProfileId = changedEntity.Entity.ChannelProfileId,
                        Operation = changedEntity.Operation,
                    });

                foreach (var changedEntity in entity.UpdatableAvailabilityGroups.ChangedEntities)
                    await eventService.Publish(new OnAvailabilityChannelProfileAssociationOperationEvent
                    {
                        MerchantId = entity.Model.MerchantId,
                        AvailabilityGroupId = changedEntity.Entity.AvailabilityGroupId,
                        ChannelProfileId = changedEntity.Entity.ChannelProfileId,
                        Operation = changedEntity.Operation,
                    });
            }

            return entities;
        }
    };
}