using Quivi.Domain.Entities.Charges;

namespace Quivi.Domain.Entities.Financing
{
    public class DepositRefundJournal : IEntity
    {
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int DepositId { get; set; }
        public required Deposit Deposit { get; set; }

        public int JournalId { get; set; }
        public required Journal Journal { get; set; }
        #endregion
    }
}
