using Quivi.Domain.Entities.Financing;

namespace Quivi.Domain.Entities.Charges
{
    public class Deposit : IEntity
    {
        public int Id => ChargeId;

        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int ConsumerId { get; set; }
        public required Person Consumer { get; set; }

        public int ChargeId { get; set; }
        public required Charge Charge { get; set; }

        public DepositCapture DepositCapture { get; set; }
        public DepositSurcharge DepositSurchage { get; set; }
        public DepositCaptureJournal DepositCaptureJournal { get; set; }
        public DepositSurchargeJournal DepositSurchargeJournal { get; set; }
        public DepositJournal DepositJournal { get; set; }
        public DepositRefundJournal DepositRefundJournal { get; set; }
        #endregion
    }
}