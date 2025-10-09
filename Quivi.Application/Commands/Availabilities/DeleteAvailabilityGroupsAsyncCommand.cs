using Quivi.Domain.Entities.Pos;
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
    public class DeleteAvailabilityGroupsAsyncCommand : ICommand<Task<IEnumerable<AvailabilityGroup>>>
    {
        public required GetAvailabilityGroupsCriteria Criteria { get; init; }
    }

    public class DeleteAvailabilityGroupsAsyncCommandHandler : ICommandHandler<DeleteAvailabilityGroupsAsyncCommand, Task<IEnumerable<AvailabilityGroup>>>
    {
        private readonly IAvailabilityGroupsRepository repository;
        private readonly IEventService eventService;

        public DeleteAvailabilityGroupsAsyncCommandHandler(IAvailabilityGroupsRepository repository,
                                                            IEventService eventService)
        {
            this.repository = repository;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<AvailabilityGroup>> Handle(DeleteAvailabilityGroupsAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeWeeklyAvailabilities = true,
                IncludeAssociatedChannelProfiles = true,
                IncludeAssociatedMenuItems = true,
            });
            if (entities.Any() == false)
                return [];

            foreach (var entity in entities)
                repository.Remove(entity);

            await repository.SaveChangesAsync();

            foreach (var entity in entities)
            {
                await eventService.Publish(new OnAvailabilityGroupOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Update,
                });

                foreach (var relatedEntity in entity.AssociatedChannelProfiles ?? [])
                    await eventService.Publish(new OnAvailabilityChannelProfileAssociationOperationEvent
                    {
                        MerchantId = entity.MerchantId,
                        AvailabilityGroupId = relatedEntity.AvailabilityGroupId,
                        ChannelProfileId = relatedEntity.ChannelProfileId,
                        Operation = EntityOperation.Delete,
                    });

                foreach (var relatedEntity in entity.AssociatedMenuItems ?? [])
                    await eventService.Publish(new OnAvailabilityMenuItemAssociationOperationEvent
                    {
                        MerchantId = entity.MerchantId,
                        AvailabilityGroupId = relatedEntity.AvailabilityGroupId,
                        MenuItemId = relatedEntity.MenuItemId,
                        Operation = EntityOperation.Delete,
                    });
            }
            return entities;
        }
    }
}