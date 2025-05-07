using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Pos
{
    public class MenuItem : IDeletableEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool Stock { get; set; }
        public decimal Price { get; set; }
        public PriceType PriceType { get; set; }
        public bool ShowWhenNotAvailable { get; set; }
        public decimal VatRate { get; set; }
        public bool IsUnavailable { get; internal set; }
        public int SortIndex { get; set; }
        public string? ImageUrl { get; set; }
        public bool HiddenFromGuestApp { get; set; }
    
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public int? LocationId { get; set; }
        public Location? Location { get; set; }

        public ICollection<MenuItemCategoryAssociation>? MenuItemCategoryAssociations { get; set; }
        public ICollection<MenuItemWeeklyAvailability>? MenuItemWeeklyAvailabilities { get; set; }
        public ICollection<MenuItemTranslation>? MenuItemTranslations { get; set; }
        public ICollection<ItemsModifierGroupsAssociation>? MenuItemModifierGroups { get; set; }
        public ICollection<MenuItemModifier>? MenuItemModifiers { get; set; }
        public ICollection<PreparationGroupItem>? PreparationGroupItems { get; set; }
        #endregion
    }
}
