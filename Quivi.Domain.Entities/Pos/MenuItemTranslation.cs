namespace Quivi.Domain.Entities.Pos
{
    public class MenuItemTranslation : IEntity, ITranslation
    {
        public Language Language { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }
        #endregion
    }
}
