using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Financing
{
    public class SettlementDetail : IEntity
    {
        public required string MerchantIban { get; set; }
        public decimal MerchantVatRate { get; set; }
        public decimal TransactionFee { get; set; }
        public decimal Amount { get; set; }
        public decimal IncludedTip { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal IncludedNetTip { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int ParentMerchantId { get; set; }
        public Merchant? ParentMerchant { get; set; }

        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public int SettlementId { get; set; }
        public Settlement? Settlement { get; set; }

        public int JournalId { get; set; }
        public Journal? Journal { get; set; }
        #endregion
    }
}