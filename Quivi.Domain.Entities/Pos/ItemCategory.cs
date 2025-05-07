using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Pos
{
    public class ItemCategory : IDeletableEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }
        public string? ImagePath { get; set; }
        public int SortIndex { get; set; }

        public DateTime? DeletedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public ICollection<MenuItemCategoryAssociation>? MenuItemCategoryAssociations { get; set; }
        public ICollection<ItemCategoryTranslation>? ItemCategoryTranslations { get; set; }
        #endregion
    }
}
