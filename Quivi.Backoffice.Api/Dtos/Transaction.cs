using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;

namespace Quivi.Backoffice.Api.Dtos
{
    public enum SynchronizationState
    {
        Failed = -1,
        Syncing = 0,
        Succeeded = 1,
    }

    public class Transaction
    {
        public required string Id { get; init; }
        public ChargeMethod ChargeMethod { get; init; }
        public DateTimeOffset CapturedDate { get; init; }
        public decimal Payment { get; init; }
        public decimal? PaymentDiscount { get; init; }
        public decimal Tip { get; init; }
        public decimal Surcharge { get; init; }
        public decimal RefundedAmount { get; init; }
        public InvoiceRefundType? InvoiceRefundType { get; init; }
        public string? Email { get; init; }
        public string? VatNumber { get; init; }
        public SynchronizationState SyncingState { get; init; }
        public string? SessionId { get; init; }
        public required string ChannelId { get; init; }
        public required string MerchantId { get; init; }
        public string? CustomChargeMethodId { get; init; }
        public required IEnumerable<TransactionItem> Items { get; init; }
    }
}