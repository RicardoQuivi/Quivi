namespace Quivi.Domain.Entities.Pos
{
    public class MenuItemModifier : IDeletableEntity
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int SortIndex { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int MenuItemModifierGroupId { get; set; }
        public ItemsModifierGroup? MenuItemModifierGroup { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }
        #endregion
    }
}