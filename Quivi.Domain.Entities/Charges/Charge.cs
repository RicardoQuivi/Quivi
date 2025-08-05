using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Entities.Pos;

namespace Quivi.Domain.Entities.Charges
{
    public class Charge : IEntity
    {
        public int Id { get; set; }

        public ChargeStatus Status { get; set; }
        public ChargePartner ChargePartner { get; set; }
        public ChargeMethod ChargeMethod { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int? MerchantAcquirerConfigurationId { get; set; }
        public MerchantAcquirerConfiguration? MerchantAcquirerConfiguration { get; set; }

        public int? ChainedChargeId { get; set; }
        public Charge? ChainedCharge { get; set; }

        public Deposit? Deposit { get; set; }
        public PosCharge? PosCharge { get; set; }
        public MerchantCustomCharge? MerchantCustomCharge { get; set; }
        public AcquirerCharge? AcquirerCharge { get; set; }
        public ICollection<MerchantInvoiceDocument>? InvoiceDocuments { get; set; }
        #endregion
    }
}
