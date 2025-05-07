using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Financing
{
    public class SettlementServiceDetail : IEntity
    {
        public int SettlementServiceDetailId { get; set; }

        public string? SubMerchantIban { get; set; }
        public decimal SubMerchantVatRate { get; set; }

        public decimal Amount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount => Amount + VatAmount;

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationship
        public int JournalId { get; set; }
        public required Journal Journal { get; set; }

        public int MerchantId { get; set; }
        public required Merchant Merchant { get; set; }
        
        public int SubMerchantId { get; set; }
        public required Merchant SubMerchant { get; set; }

        public int SettlementId { get; set; }
        public required Settlement Settlement { get; set; }

        public int MerchantServiceId { get; set; }
        public required MerchantService MerchantService { get; set; }
        #endregion
    }
}
