namespace Quivi.Domain.Entities.Pos
{
    public class OrderConfigurableFieldChannelProfileAssociation : IEntity
    {
        public int OrderConfigurableFieldId { get; set; }
        public int ChannelProfileId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public OrderConfigurableField? OrderConfigurableField { get; set; }
        public ChannelProfile? ChannelProfile { get; set; }
        #endregion
    }
}