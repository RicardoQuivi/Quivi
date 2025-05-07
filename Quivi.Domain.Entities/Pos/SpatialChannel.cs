namespace Quivi.Domain.Entities.Pos
{
    public class SpatialChannel : IDeletableEntity
    {
        public int Id { get; set; }

        public decimal RelativePositionX { get; set; }
        public decimal RelativePositionY { get; set; }
        public SpatialChannelShape Shape { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int ChannelId { get; set; }
        public required Channel Channel { get; set; }
        #endregion
    }

    public enum SpatialChannelShape
    {
        Rectangle = 0,
        Circle = 1,
    }
}
