using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Charges
{
    public class DepositSurcharge
    {
        public decimal Amount { get; set; }
        public decimal AppliedValue { get; set; }
        public FeeUnit AppliedUnit { get; set; }

        #region Relationships
        public int DepositId { get; set; }
        public Deposit? Deposit { get; set; }
        #endregion
    }
}
