namespace Quivi.Guests.Api.Dtos
{
    public class Merchant
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string LogoUrl { get; init; }
        public bool SurchargeFeesMayApply { get; init; }
        public bool FreePayment { get; init; }
        public bool ItemSelectionPayment { get; init; }
        public bool SplitBillPayment { get; init; }
        public bool EnforceTip { get; init; }
        public bool ShowPaymentNotes { get; init; }
        public bool AllowsIgnoreBill { get; init; }
        public bool Inactive { get; init; }
        public required IEnumerable<Fee> Fees { get; init; }
    }
}