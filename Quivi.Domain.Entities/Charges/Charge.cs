using Quivi.Domain.Entities.Pos;

namespace Quivi.Domain.Entities.Charges
{
    public class Charge : IEntity
    {
        public int ChargeId { get; set; }

        public ChargeStatus Status { get; set; }
        public ChargePartner? ChargePartner { get; set; }
        public ChargeMethod? ChargeMethod { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int? ChainedChargeId { get; set; }
        public Charge? ChainedCharge { get; set; }

        public Deposit? Deposit { get; set; }
        public CardCharge? CardCharge { get; set; }
        public TerminalCharge? TerminalCharge { get; set; }
        public PosCharge? PosCharge { get; set; }
        public MbWayCharge? MbWayCharge { get; set; }
        public TicketMobileCharge? TicketMobileCharge { get; set; }
        public MerchantCustomCharge? MerchantCustomCharge { get; set; }
        public ICollection<MerchantInvoiceDocument>? InvoiceDocuments { get; set; }
        #endregion
    }
}
