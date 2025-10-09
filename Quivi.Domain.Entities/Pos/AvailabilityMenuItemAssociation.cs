namespace Quivi.Domain.Entities.Pos
{
    public class AvailabilityMenuItemAssociation : IEntity
    {
        public int AvailabilityGroupId { get; set; }
        public int MenuItemId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public AvailabilityGroup? AvailabilityGroup { get; set; }
        public MenuItem? MenuItem { get; set; }
        #endregion
    }
}