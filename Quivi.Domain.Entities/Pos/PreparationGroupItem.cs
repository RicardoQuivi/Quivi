namespace Quivi.Domain.Entities.Pos
{
    public class PreparationGroupItem : IEntity
    {
        public int Id { get; set; }
        public int OriginalQuantity { get; set; }
        public int RemainingQuantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int? ParentPreparationGroupItemId { get; set; }
        public PreparationGroupItem? ParentPreparationGroupItem { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public int? LocationId { get; set; }
        public Location? Location { get; set; }

        public int PreparationGroupId { get; set; }
        public PreparationGroup? PreparationGroup { get; set; }

        public ICollection<PreparationGroupItem>? Extras { get; set; }
        #endregion
    }
}
