using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.MenuItems;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.MenuItems
{
    public interface IMenuItemTranslation
    {
        Language Language { get; }
        string Name { get; set; }
        string? Description { get; set; }
    }

    public interface IUpdatableItemCategory : IUpdatableEntity
    {
        public int Id { get; }
    }

    public interface IUpdatableModifierGroup : IUpdatableEntity
    {
        public int Id { get; }
    }

    public interface IUpdatableMenuItem : IUpdatableEntity
    {
        int MerchantId { get; }
        int Id { get; }
        string Name { get; set; }
        string? Description { get; set; }
        decimal Price { get; set; }
        PriceType PriceType { get; set; }
        decimal VatRate { get; set; }
        int? LocationId { get; set; }
        string? ImageUrl { get; set; }
        int SortIndex { get; set; }
        bool HasStock { get; set; }

        IUpdatableTranslations<IMenuItemTranslation> Translations { get; }
        IUpdatableRelationship<IUpdatableItemCategory, int> Categories { get; }
        IUpdatableRelationship<IUpdatableModifierGroup, int> ModifierGroups { get; }
    }


    public class UpdateMenuItemAsyncCommand : AUpdateAsyncCommand<IEnumerable<MenuItem>, IUpdatableMenuItem>
    {
        public required GetMenuItemsCriteria Criteria { get; init; }
    }

    public class UpdateMenuItemAsyncCommandHandler : ICommandHandler<UpdateMenuItemAsyncCommand, Task<IEnumerable<MenuItem>>>
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

        private class UpdatableMenuItemTranslation : IMenuItemTranslation, IUpdatableEntity
        {
            public readonly MenuItemTranslation Model;
            private readonly bool isNew;
            private readonly string originalName;
            private readonly string? originalDescription;

            public UpdatableMenuItemTranslation(MenuItemTranslation model)
            {
                Model = model;
                isNew = model.MenuItemId == 0;
                originalName = model.Name;
                originalDescription = model.Description;
            }

            public Language Language => Model.Language;
            public string Name { get => Model.Name; set => Model.Name = value; }
            public string? Description { get => Model.Description; set => Model.Description = value; }
            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    if (originalName != Model.Name)
                        return true;

                    if (originalDescription != Model.Description)
                        return true;

                    return false;
                }
            }
        }

        private class UpdatableCategory : IUpdatableItemCategory
        {
            public readonly MenuItemCategoryAssociation Model;
            private readonly bool isNew;

            public UpdatableCategory(MenuItemCategoryAssociation model)
            {
                this.Model = model;
                isNew = model.MenuItemId == 0;
            }
            public int Id => Model.ItemCategoryId;

            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    return false;
                }
            }
        }

        private class UpdatableModifierGroup : IUpdatableModifierGroup
        {
            public readonly ItemsModifierGroupsAssociation Model;
            private readonly bool isNew;

            public UpdatableModifierGroup(ItemsModifierGroupsAssociation model)
            {
                this.Model = model;
                isNew = model.MenuItemId == 0;
            }
            public int Id => Model.MenuItemModifierGroupId;

            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    return false;
                }
            }
        }

        private class UpdatableMenuItem : AUpdatableTranslatableEntity<MenuItemTranslation, UpdatableMenuItemTranslation, IMenuItemTranslation>, IUpdatableMenuItem
        {
            private readonly MenuItem model;
            private readonly UpdatableRelationshipEntity<MenuItemCategoryAssociation, IUpdatableItemCategory, int> updatableCategories;
            private readonly UpdatableRelationshipEntity<ItemsModifierGroupsAssociation, IUpdatableModifierGroup, int> updatableGroups;
            private readonly string originalName;
            private readonly string? originalDescription;
            private readonly decimal originalPrice;
            private readonly PriceType originalPriceType;
            private readonly decimal originalVatRate;
            private readonly int? originalLocationId;
            private readonly string? originalImageUrl;
            private readonly int originalSortIndex;
            private readonly bool originalStock;

            public UpdatableMenuItem(MenuItem model, DateTime now) : base(model.MenuItemTranslations ?? [], t => new UpdatableMenuItemTranslation(t), () => new MenuItemTranslation
            {
                Name = "",
                Description = "",

                CreatedDate = now,
                ModifiedDate = now,
                DeletedDate = null,
            })
            {
                this.model = model;
                this.updatableCategories = new UpdatableRelationshipEntity<MenuItemCategoryAssociation, IUpdatableItemCategory, int>(model.MenuItemCategoryAssociations!, m => m.ItemCategoryId, t => new UpdatableCategory(t), (id) => new MenuItemCategoryAssociation
                {
                    ItemCategoryId = id,
                    MenuItem = this.model,
                    MenuItemId = this.model.Id,
                    SortIndex = 0,
                    CreatedDate = now,
                    ModifiedDate = now,
                });
                this.updatableGroups = new UpdatableRelationshipEntity<ItemsModifierGroupsAssociation, IUpdatableModifierGroup, int>(model.MenuItemModifierGroups!, m => m.MenuItemModifierGroupId, t => new UpdatableModifierGroup(t), (id) => new ItemsModifierGroupsAssociation
                {
                    MenuItemModifierGroupId = id,
                    MenuItem = this.model,
                    MenuItemId = this.model.Id,
                    SortIndex = 0,
                    CreatedDate = now,
                    ModifiedDate = now,
                });
                originalName = model.Name;
                originalDescription = model.Description;
                originalPrice = model.Price;
                originalPriceType = model.PriceType;
                originalVatRate = model.VatRate;
                originalLocationId = model.LocationId;
                originalImageUrl = model.ImageUrl;
                originalSortIndex = model.SortIndex;
                originalStock = model.Stock;
            }

            public int MerchantId => model.MerchantId;
            public int Id => model.Id;
            public string Name { get => model.Name; set => model.Name = value; }
            public string? Description { get => model.Description; set => model.Description = value; }
            public decimal Price { get => model.Price; set => model.Price = value; }
            public PriceType PriceType { get => model.PriceType; set => model.PriceType = value; }
            public decimal VatRate { get => model.VatRate; set => model.VatRate = value; }
            public int? LocationId { get => model.LocationId; set => model.LocationId = value; }
            public string? ImageUrl { get => model.ImageUrl; set => model.ImageUrl = value; }
            public int SortIndex { get => model.SortIndex; set => model.SortIndex = value; }
            public bool HasStock { get => model.Stock; set => model.Stock = value; }
            public IUpdatableRelationship<IUpdatableItemCategory, int> Categories => updatableCategories;
            public IUpdatableRelationship<IUpdatableModifierGroup, int> ModifierGroups => updatableGroups;

            public override bool HasChanges
            {
                get
                {
                    if (originalName != model.Name)
                        return true;

                    if (originalDescription != model.Description)
                        return true;

                    if (originalPrice != model.Price)
                        return true;

                    if (originalPriceType != model.PriceType)
                        return true;

                    if (originalVatRate != model.VatRate)
                        return true;

                    if (originalLocationId != model.LocationId)
                        return true;

                    if (originalImageUrl != model.ImageUrl)
                        return true;

                    if (originalSortIndex != model.SortIndex)
                        return true;

                    if (originalStock != model.Stock)
                        return true;

                    if (updatableCategories.HasChanges)
                        return true;

                    return base.HasChanges;
                }
            }

        }


        public async Task<IEnumerable<MenuItem>> Handle(UpdateMenuItemAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeTranslations = true,
                IncludeMenuItemCategoryAssociations = true,
                IncludeModifierGroups = true,
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
                await eventService.Publish(new OnMenuItemOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Update,
                });

            return entities;
        }
    }
}
