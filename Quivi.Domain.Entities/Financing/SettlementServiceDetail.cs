using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Financing
{
    public class SettlementServiceDetail : IEntity
    {
        public string? MerchantIban { get; set; }
        public decimal MerchantVatRate { get; set; }

        public decimal Amount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount => Amount + VatAmount;

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationship
        public int JournalId { get; set; }
        public Journal? Journal { get; set; }

        public int ParentMerchantId { get; set; }
        public Merchant? ParentMerchant { get; set; }

        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public int SettlementId { get; set; }
        public Settlement? Settlement { get; set; }

        public int MerchantServiceId { get; set; }
        public MerchantService? MerchantService { get; set; }
        #endregion
    }
}
