namespace Quivi.Domain.Entities.Charges
{
    public class MbWayCharge : IEntity
    {
        public required string TransactionId { get; set; }
        public required string PhoneNumber { get; set; }
        public DateTime LastStatusCheckDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int ChargeId { get; set; }
        public required Charge Charge { get; set; }
        #endregion
    }
}
