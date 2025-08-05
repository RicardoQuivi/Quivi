namespace Quivi.Domain.Entities.Charges
{
    public class AcquirerCharge : IEntity
    {
        public int Id => ChargeId;
        public string? AcquirerId { get; set; }

        public required string Culture { get; set; }

        public string? AdditionalJsonContext { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int ChargeId { get; set; }
        public Charge? Charge { get; set; }
        #endregion
    }
}