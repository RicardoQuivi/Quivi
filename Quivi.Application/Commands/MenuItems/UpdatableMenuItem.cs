using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;

namespace Quivi.Application.Commands.MenuItems
{
    public class UpdatableMenuItemTranslation : IMenuItemTranslation, IUpdatableEntity
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

    public class UpdatableMenuItem : AUpdatableTranslatableEntity<MenuItemTranslation, UpdatableMenuItemTranslation, IMenuItemTranslation>, IUpdatableMenuItem
    {
        private class UpdatableAvailabilityGroup : IUpdatableAvailabilityGroup
        {
            public readonly AvailabilityMenuItemAssociation Model;
            private readonly bool isNew;

            public UpdatableAvailabilityGroup(AvailabilityMenuItemAssociation model)
            {
                this.Model = model;
                isNew = model.MenuItemId == 0;
            }
            public int Id => Model.AvailabilityGroupId;

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

        public readonly MenuItem Model;
        public readonly UpdatableRelationshipEntity<MenuItemCategoryAssociation, IUpdatableItemCategory, int> UpdatableCategories;
        public readonly UpdatableRelationshipEntity<ItemsModifierGroupsAssociation, IUpdatableModifierGroup, int> UpdatableModifierGroups;
        public readonly UpdatableRelationshipEntity<AvailabilityMenuItemAssociation, IUpdatableAvailabilityGroup, int> UpdatableAvailabilityGroups;

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
            this.Model = model;
            this.UpdatableCategories = new UpdatableRelationshipEntity<MenuItemCategoryAssociation, IUpdatableItemCategory, int>(model.MenuItemCategoryAssociations!, m => m.ItemCategoryId, t => new UpdatableCategory(t), (id) => new MenuItemCategoryAssociation
            {
                ItemCategoryId = id,
                MenuItem = this.Model,
                MenuItemId = this.Model.Id,
                SortIndex = 0,
                CreatedDate = now,
                ModifiedDate = now,
            });
            this.UpdatableModifierGroups = new UpdatableRelationshipEntity<ItemsModifierGroupsAssociation, IUpdatableModifierGroup, int>(model.MenuItemModifierGroups!, m => m.MenuItemModifierGroupId, t => new UpdatableModifierGroup(t), (id) => new ItemsModifierGroupsAssociation
            {
                MenuItemModifierGroupId = id,
                MenuItem = this.Model,
                MenuItemId = this.Model.Id,
                SortIndex = 0,
                CreatedDate = now,
                ModifiedDate = now,
            });
            this.UpdatableAvailabilityGroups = new UpdatableRelationshipEntity<AvailabilityMenuItemAssociation, IUpdatableAvailabilityGroup, int>(model.AssociatedAvailabilityGroups!, m => m.AvailabilityGroupId, t => new UpdatableAvailabilityGroup(t), (id) => new AvailabilityMenuItemAssociation
            {
                AvailabilityGroupId = id,
                MenuItem = this.Model,
                MenuItemId = this.Model.Id,
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

        public int MerchantId => Model.MerchantId;
        public int Id => Model.Id;
        public string Name { get => Model.Name; set => Model.Name = value; }
        public string? Description { get => Model.Description; set => Model.Description = value; }
        public decimal Price { get => Model.Price; set => Model.Price = value; }
        public PriceType PriceType { get => Model.PriceType; set => Model.PriceType = value; }
        public decimal VatRate { get => Model.VatRate; set => Model.VatRate = value; }
        public int? LocationId { get => Model.LocationId; set => Model.LocationId = value; }
        public string? ImageUrl { get => Model.ImageUrl; set => Model.ImageUrl = value; }
        public int SortIndex { get => Model.SortIndex; set => Model.SortIndex = value; }
        public bool HasStock { get => Model.Stock; set => Model.Stock = value; }
        public IUpdatableRelationship<IUpdatableItemCategory, int> Categories => UpdatableCategories;
        public IUpdatableRelationship<IUpdatableModifierGroup, int> ModifierGroups => UpdatableModifierGroups;
        public IUpdatableRelationship<IUpdatableAvailabilityGroup, int> AvailabilityGroups => UpdatableAvailabilityGroups;

        public override bool HasChanges
        {
            get
            {
                if (originalName != Model.Name)
                    return true;

                if (originalDescription != Model.Description)
                    return true;

                if (originalPrice != Model.Price)
                    return true;

                if (originalPriceType != Model.PriceType)
                    return true;

                if (originalVatRate != Model.VatRate)
                    return true;

                if (originalLocationId != Model.LocationId)
                    return true;

                if (originalImageUrl != Model.ImageUrl)
                    return true;

                if (originalSortIndex != Model.SortIndex)
                    return true;

                if (originalStock != Model.Stock)
                    return true;

                if (UpdatableCategories.HasChanges)
                    return true;

                if (UpdatableAvailabilityGroups.HasChanges)
                    return true;

                return base.HasChanges;
            }
        }
    }
}