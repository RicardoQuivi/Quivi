using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.ItemsModifierGroups;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.ItemsModifierGroups
{
    public interface IItemsModifierGroupTranslation
    {
        Language Language { get; }
        string Name { get; set; }
    }

    public interface IUpdatableModifierItem : IUpdatableEntity
    {
        public int Id { get; }
        public decimal Price { get; set; }
        public int SortIndex { get; set; }
    }

    public interface IUpdatableItemsModifierGroup : IUpdatableEntity
    {
        int MerchantId { get; }
        int Id { get; }
        string Name { get; set; }
        int MinSelection { get; set; }
        int MaxSelection { get; set; }

        IUpdatableTranslations<IItemsModifierGroupTranslation> Translations { get; }
        IUpdatableRelationship<IUpdatableModifierItem, int> MenuItems { get; }
    }


    public class UpdateItemsModifierGroupAsyncCommand : AUpdateAsyncCommand<IEnumerable<ItemsModifierGroup>, IUpdatableItemsModifierGroup>
    {
        public required GetItemsModifierGroupsCriteria Criteria { get; init; }
    }

    public class UpdateMenuItemAsyncCommandHandler : ICommandHandler<UpdateItemsModifierGroupAsyncCommand, Task<IEnumerable<ItemsModifierGroup>>>
    {
        private readonly IItemsModifierGroupsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateMenuItemAsyncCommandHandler(IItemsModifierGroupsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        private class UpdatableModifierTranslation : IItemsModifierGroupTranslation, IUpdatableEntity
        {
            public readonly ItemsModifierGroupTranslation Model;
            private readonly bool isNew;
            private readonly string originalName;

            public UpdatableModifierTranslation(ItemsModifierGroupTranslation model)
            {
                Model = model;
                isNew = model.MenuItemModifierGroupId == 0;
                originalName = model.Name;
            }

            public Language Language => Model.Language;
            public string Name { get => Model.Name; set => Model.Name = value; }
            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    if (originalName != Model.Name)
                        return true;

                    return false;
                }
            }
        }

        private class UpdatableModifierItem : IUpdatableModifierItem
        {
            public readonly MenuItemModifier Model;
            private readonly bool isNew;
            private readonly decimal originalPrice;
            private readonly int originalSortIndex;

            public UpdatableModifierItem(MenuItemModifier model)
            {
                this.Model = model;
                isNew = model.MenuItemId == 0;
                originalPrice = model.Price;
                originalSortIndex = model.SortIndex;
            }
            public int Id => Model.MenuItemId;
            public decimal Price { get => Model.Price; set => Model.Price = value; }
            public int SortIndex { get => Model.SortIndex; set => Model.SortIndex = value; }
            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    if(originalPrice != Model.Price)
                        return true;

                    if(originalSortIndex != Model.SortIndex)
                        return true;

                    return false;
                }
            }
        }

        private class UpdatableModifierGroup : AUpdatableTranslatableEntity<ItemsModifierGroupTranslation, UpdatableModifierTranslation, IItemsModifierGroupTranslation>, IUpdatableItemsModifierGroup
        {
            private readonly ItemsModifierGroup model;
            private readonly UpdatableRelationshipEntity<MenuItemModifier, IUpdatableModifierItem, int> updatebleItems;
            private readonly string originalName;
            private readonly int originalMinSelection;
            private readonly int originalMaxSelection;

            public UpdatableModifierGroup(ItemsModifierGroup model, DateTime now) : base(model.ItemsModifierGroupTranslations ?? [], t => new UpdatableModifierTranslation(t), () => new ItemsModifierGroupTranslation
            {
                Name = "",
                
                CreatedDate = now,
                ModifiedDate = now,
                DeletedDate = null,
            })
            {
                this.model = model;
                this.updatebleItems = new (model.MenuItemModifiers!, m => m.MenuItemId, t => new UpdatableModifierItem(t), (id) => new MenuItemModifier
                {
                    MenuItemId = id,

                    MenuItemModifierGroup = this.model,
                    MenuItemModifierGroupId = this.model.Id,

                    Price = 0.0m,
                    SortIndex = 0,

                    CreatedDate = now,
                    ModifiedDate = now,
                    DeletedDate = null,
                });
                originalName = model.Name;
                originalMinSelection = model.MinSelection;
                originalMaxSelection = model.MaxSelection;
            }

            public int MerchantId => model.MerchantId;
            public int Id => model.Id;
            public string Name { get => model.Name; set => model.Name = value; }
            public int MinSelection { get => model.MinSelection; set => model.MinSelection = value; }
            public int MaxSelection { get => model.MaxSelection; set => model.MaxSelection = value; }
            public IUpdatableRelationship<IUpdatableModifierItem, int> MenuItems => updatebleItems;

            public override bool HasChanges
            {
                get
                {
                    if (originalName != model.Name)
                        return true;

                    if (originalMinSelection != model.MinSelection)
                        return true;

                    if (originalMaxSelection != model.MaxSelection)
                        return true;

                    if (updatebleItems.HasChanges)
                        return true;

                    return base.HasChanges;
                }
            }
        }


        public async Task<IEnumerable<ItemsModifierGroup>> Handle(UpdateItemsModifierGroupAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeTranslations = true,
                IncludeModifiers = true,
                IsDeleted = false,
            });
            if (!entities.Any())
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            var changedEntities = new List<UpdatableModifierGroup>();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableModifierGroup(entity, now);
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
                await eventService.Publish(new OnItemsModifierGroupOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Update,
                });

            return entities;
        }
    }
}
