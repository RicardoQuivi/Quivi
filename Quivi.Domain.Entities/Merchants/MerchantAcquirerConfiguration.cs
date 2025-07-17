using Quivi.Domain.Entities.Charges;

namespace Quivi.Domain.Entities.Merchants
{
    public class MerchantAcquirerConfiguration : IDeletableEntity
    {
        public int Id { get; set; }
        public ChargePartner ChargePartner { get; set; }
        public ChargeMethod ChargeMethod { get; set; }

        public string? ApiKey { get; set; }
        public string? EntityId { get; set; }
        public string? TerminalId { get; set; }
        public string? WebhookSecret { get; set; }
        public string? PublicKey { get; set; }
        public bool ExternallySettled { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }
        #endregion
    }
}