namespace Quivi.Domain.Repositories.EntityFramework.Models
{
    public class MerchantSettlementDetail
    {
        public int JournalId { get; init; }
        public int SettlementId { get; init; }

        public DateTime TransactionDate { get; init; }

        public int ParentMerchantId { get; init; }
        public int MerchantId { get; init; }

        public decimal GrossAmount { get; init; }
        public decimal GrossTip { get; init; }
        public decimal GrossTotal => GrossAmount + GrossTip;

        public decimal NetAmount { get; init; }
        public decimal NetTip { get; init; }
        public decimal NetTotal => NetAmount + NetTip;
    }
}
