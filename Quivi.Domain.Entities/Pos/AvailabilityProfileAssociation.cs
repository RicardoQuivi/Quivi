namespace Quivi.Domain.Entities.Pos
{
    public class AvailabilityProfileAssociation : IEntity
    {
        public int AvailabilityGroupId { get; set; }
        public int ChannelProfileId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public AvailabilityGroup? AvailabilityGroup { get; set; }
        public ChannelProfile? ChannelProfile { get; set; }
        #endregion
    }
}