using Quivi.Application.Commands.Availabilities;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroupChannelProfileAssociations;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroupMenuItemAssociations;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroups;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.AvailabilityGroups
{
    public class AddAvailabilityGroupAsyncCommand : ICommand<Task<AvailabilityGroup?>>
    {
        public int MerchantId { get; init; }
        public required Action OnInvalidName { get; init; }
        public required Func<IUpdatableAvailabilityGroup, Task> OnCreate { get; init; }
    }

    public class AddAvailabilityGroupAsyncCommandHandler : ICommandHandler<AddAvailabilityGroupAsyncCommand, Task<AvailabilityGroup?>>
    {
        private readonly IAvailabilityGroupsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public AddAvailabilityGroupAsyncCommandHandler(IAvailabilityGroupsRepository repository,
                                                        IDateTimeProvider dateTimeProvider,
                                                        IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<AvailabilityGroup?> Handle(AddAvailabilityGroupAsyncCommand command)
        {
            var now = dateTimeProvider.GetUtcNow();
            var entity = new AvailabilityGroup
            {
                MerchantId = command.MerchantId,
                Name = string.Empty,

                WeeklyAvailabilities = new List<WeeklyAvailability>(),
                AssociatedMenuItems = new List<AvailabilityMenuItemAssociation>(),
                AssociatedChannelProfiles = new List<AvailabilityProfileAssociation>(),

                CreatedDate = now,
                ModifiedDate = now,
            };

            var updatableEntity = new UpdatableAvailabilityGroup(entity, now);
            await command.OnCreate(updatableEntity);

            if (string.IsNullOrWhiteSpace(entity.Name))
            {
                command.OnInvalidName();
                return null;
            }

            repository.Add(entity);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnAvailabilityGroupOperationEvent
            {
                Id = entity.Id,
                MerchantId = entity.MerchantId,
                Operation = EntityOperation.Create,
            });

            foreach (var changedEntity in updatableEntity.UpdatableChannelProfileAssociations.ChangedEntities)
                await eventService.Publish(new OnAvailabilityChannelProfileAssociationOperationEvent
                {
                    MerchantId = entity.MerchantId,
                    AvailabilityGroupId = changedEntity.Entity.AvailabilityGroupId,
                    ChannelProfileId = changedEntity.Entity.ChannelProfileId,
                    Operation = changedEntity.Operation,
                });

            foreach (var changedEntity in updatableEntity.UpdatableMenuItemsAssociations.ChangedEntities)
                await eventService.Publish(new OnAvailabilityMenuItemAssociationOperationEvent
                {
                    MerchantId = entity.MerchantId,
                    AvailabilityGroupId = changedEntity.Entity.AvailabilityGroupId,
                    MenuItemId = changedEntity.Entity.MenuItemId,
                    Operation = changedEntity.Operation,
                });
            return entity;
        }
    };
}