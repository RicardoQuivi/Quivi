namespace Quivi.Domain.Entities.Pos
{
    public class ItemCategoryTranslation : IEntity, ITranslation
    {
        public Language Language { get; set; }

        public required string Name { get; set; }

        public DateTime? DeletedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int ItemCategoryId { get; set; }
        public ItemCategory? ItemCategory { get; set; }
        #endregion
    }
}