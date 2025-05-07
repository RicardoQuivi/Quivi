namespace Quivi.Domain.Entities.Pos
{
    public class ItemsModifierGroupsAssociation : IEntity
    {
        public int SortIndex { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public int MenuItemModifierGroupId { get; set; }
        public ItemsModifierGroup? MenuItemModifierGroup { get; set; }
        #endregion
    }
}