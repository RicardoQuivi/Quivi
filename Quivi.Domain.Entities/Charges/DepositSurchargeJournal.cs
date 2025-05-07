using Quivi.Domain.Entities.Financing;

namespace Quivi.Domain.Entities.Charges
{
    public class DepositSurchargeJournal
    {
        #region Relationships
        public int DepositId { get; set; }
        public required Deposit Deposit { get; set; }

        public int JournalId { get; set; }
        public required Journal Journal { get; set; }
        #endregion
    }
}
