namespace Quivi.Domain.Entities.Pos
{
    public class ItemsModifierGroupTranslation : IEntity, ITranslation
    {
        public Language Language { get; set; }
        public required string Name { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int MenuItemModifierGroupId { get; set; }
        public ItemsModifierGroup? MenuItemModifierGroup { get; set; }
        #endregion
    }
}