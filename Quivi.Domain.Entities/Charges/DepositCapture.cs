using Quivi.Domain.Entities.Financing;

namespace Quivi.Domain.Entities.Charges
{
    public class DepositCapture
    {
        public JournalType Type { get; set; }
        public decimal Amount { get; set; }

        #region Relationships
        public int DepositId { get; set; }
        public required Deposit Deposit { get; set; }

        public int PersonId { get; set; }
        public required Person Person { get; set; }
        #endregion
    }
}