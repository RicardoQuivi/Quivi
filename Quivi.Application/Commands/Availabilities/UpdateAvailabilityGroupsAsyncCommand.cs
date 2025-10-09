using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroupChannelProfileAssociations;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroupMenuItemAssociations;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroups;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Availabilities
{
    public class UpdateAvailabilityGroupsAsyncCommand : AUpdateAsyncCommand<IEnumerable<AvailabilityGroup>, IUpdatableAvailabilityGroup>
    {
        public required GetAvailabilityGroupsCriteria Criteria { get; init; }
        public required Action<IUpdatableAvailabilityGroup> OnInvalidName { get; init; }
    }

    public class UpdateAvailabilityGroupsAsyncCommandHandler : ICommandHandler<UpdateAvailabilityGroupsAsyncCommand, Task<IEnumerable<AvailabilityGroup>>>
    {
        private readonly IAvailabilityGroupsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateAvailabilityGroupsAsyncCommandHandler(IAvailabilityGroupsRepository repository,
                                                            IDateTimeProvider dateTimeProvider,
                                                            IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<AvailabilityGroup>> Handle(UpdateAvailabilityGroupsAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeWeeklyAvailabilities = true,
                IncludeAssociatedChannelProfiles = true,
                IncludeAssociatedMenuItems = true,
            });
            if (entities.Any() == false)
                return [];

            var now = dateTimeProvider.GetUtcNow();
            List<UpdatableAvailabilityGroup> changedEntities = [];
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableAvailabilityGroup(entity, now);
                await command.UpdateAction.Invoke(updatableEntity);
                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;
                    changedEntities.Add(updatableEntity);
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            if (IsNameValid(command, changedEntities) == false)
                return [];

            await repository.SaveChangesAsync();

            foreach (var entity in changedEntities)
            {
                await eventService.Publish(new OnAvailabilityGroupOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Update,
                });

                foreach (var changedEntity in entity.UpdatableChannelProfileAssociations.ChangedEntities)
                    await eventService.Publish(new OnAvailabilityChannelProfileAssociationOperationEvent
                    {
                        MerchantId = entity.MerchantId,
                        AvailabilityGroupId = changedEntity.Entity.AvailabilityGroupId,
                        ChannelProfileId = changedEntity.Entity.ChannelProfileId,
                        Operation = changedEntity.Operation,
                    });

                foreach (var changedEntity in entity.UpdatableMenuItemsAssociations.ChangedEntities)
                    await eventService.Publish(new OnAvailabilityMenuItemAssociationOperationEvent
                    {
                        MerchantId = entity.MerchantId,
                        AvailabilityGroupId = changedEntity.Entity.AvailabilityGroupId,
                        MenuItemId = changedEntity.Entity.MenuItemId,
                        Operation = changedEntity.Operation,
                    });
            }
            return entities;
        }

        private bool IsNameValid(UpdateAvailabilityGroupsAsyncCommand command, IEnumerable<UpdatableAvailabilityGroup> updatableAvailabilities)
        {
            var updatedAvailabilitiesWithNameChanges = updatableAvailabilities.Where(n => n.NameChanged);

            bool hasInvalid = false;
            List<UpdatableAvailabilityGroup> entitiesWithValidName = [];
            foreach (var updatableAvailability in updatedAvailabilitiesWithNameChanges)
            {
                if (string.IsNullOrWhiteSpace(updatableAvailability.Name))
                {
                    command.OnInvalidName(updatableAvailability);
                    hasInvalid = true;
                    continue;
                }

                entitiesWithValidName.Add(updatableAvailability);
            }

            return hasInvalid == false;
        }
    }
}