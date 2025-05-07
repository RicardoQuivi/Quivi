namespace Quivi.Domain.Entities.Pos
{
    public class Review : IEntity
    {
        public int Stars { get; set; }
        public string? Comment { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int PosChargeId { get; set; }
        public required PosCharge PosCharge { get; set; }
        #endregion
    }
}