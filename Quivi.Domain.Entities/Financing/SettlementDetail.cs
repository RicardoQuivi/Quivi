using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Financing
{
    public class SettlementDetail : IEntity
    {
        public int SettlementDetailId { get; set; }

        public required string SubMerchantIban { get; set; }
        public decimal SubMerchantVatRate { get; set; }
        public decimal TransactionFee { get; set; }
        public int SettlementDays { get; set; }
        public DateTime SettlementDate { get; set; }
        public decimal Amount { get; set; }
        public decimal IncludedTip { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal IncludedNetTip { get; set; }
        public DateTime DatetimeUTC { get; set; }

        //TODO: Removed default value
        public ChargeMethod ChargeMethod { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public required Merchant Merchant { get; set; }

        public int SubMerchantId { get; set; }
        public required Merchant SubMerchant { get; set; }
        
        public int SettlementId { get; set; }
        public required Settlement Settlement { get; set; }

        public int JournalId { get; set; }
        public required Journal Journal { get; set; }
        #endregion
    }
}