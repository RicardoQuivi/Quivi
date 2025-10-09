using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroupMenuItemAssociations;
using Quivi.Infrastructure.Abstractions.Events.Data.MenuItems;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.MenuItems
{
    public class UpdateMenuItemsAsyncCommand : AUpdateAsyncCommand<IEnumerable<MenuItem>, IUpdatableMenuItem>
    {
        public required GetMenuItemsCriteria Criteria { get; init; }
    }

    public class UpdateMenuItemAsyncCommandHandler : ICommandHandler<UpdateMenuItemsAsyncCommand, Task<IEnumerable<MenuItem>>>
    {
        private readonly IMenuItemsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateMenuItemAsyncCommandHandler(IMenuItemsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<MenuItem>> Handle(UpdateMenuItemsAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeTranslations = true,
                IncludeMenuItemCategoryAssociations = true,
                IncludeModifierGroupsAssociations = true,
                IsDeleted = false,
            });
            if (!entities.Any())
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            var changedEntities = new List<UpdatableMenuItem>();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableMenuItem(entity, now);
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
                await eventService.Publish(new OnMenuItemOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Update,
                });

                foreach (var changedEntity in entity.UpdatableAvailabilityGroups.ChangedEntities)
                    await eventService.Publish(new OnAvailabilityMenuItemAssociationOperationEvent
                    {
                        MerchantId = entity.Model.MerchantId,
                        AvailabilityGroupId = changedEntity.Entity.AvailabilityGroupId,
                        MenuItemId = changedEntity.Entity.MenuItemId,
                        Operation = changedEntity.Operation,
                    });
            }

            return entities;
        }
    }
}
