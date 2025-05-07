using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Pos
{
    public class ItemsModifierGroup : IDeletableEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }
        public int MinSelection { get; set; }
        public int MaxSelection { get; set; }

        public DateTime? DeletedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }


        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public ICollection<ItemsModifierGroupsAssociation>? ItemsModifierGroupsAssociation { get; set; }
        public ICollection<MenuItemModifier>? MenuItemModifiers { get; set; }
        public ICollection<ItemsModifierGroupTranslation>? ItemsModifierGroupTranslations { get; set; }
        #endregion
    }
}