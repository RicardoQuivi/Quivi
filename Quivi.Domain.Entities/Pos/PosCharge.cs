using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Pos
{
    public class PosCharge : IEntity
    {
        public int Id => ChargeId;

        public decimal Total { get; set; }
        public decimal SurchargeFeeAmount { get; set; }
        public decimal Payment { get; set; }
        public decimal Tip { get; set; }
        public string? Email { get; set; }
        public string? VatNumber { get; set; }
        public decimal? TotalRefund { get; set; }
        public decimal? PaymentRefund { get; set; }
        public decimal? TipRefund { get; set; }
        public string? Observations { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? CaptureDate { get; set; }
        public InvoiceRefundType? InvoiceRefundType { get; set; }
        public string? RefundReason { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int ChargeId { get; set; }
        public Charge? Charge { get; set; }

        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public int ChannelId { get; set; }
        public Channel? Channel { get; set; }

        public int? SessionId { get; set; }
        public Session? Session { get; set; }

        public int? LocationId { get; set; }
        public Location? Location { get; set; }

        public int? RefundEmployeeId { get; set; }
        public Employee? RefundEmployee { get; set; }

        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public Review? Review { get; set; }

        public ICollection<PosChargeSelectedMenuItem>? PosChargeSelectedMenuItems { get; set; }
        public ICollection<PosChargeInvoiceItem>? PosChargeInvoiceItems { get; set; }
        public ICollection<PosChargeSyncAttempt>? PosChargeSyncAttempts { get; set; }
        #endregion
    }
}