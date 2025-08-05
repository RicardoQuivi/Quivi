using Quivi.Domain.Entities.Charges;

namespace Quivi.Guests.Api.Dtos
{
    public class Transaction
    {
        public required string Id { get; init; }
        public decimal Total { get; init; }
        public decimal Payment { get; init; }
        public decimal Tip { get; init; }
        public decimal Surcharge { get; init; }
        public decimal SyncedAmount { get; init; }
        public DateTimeOffset? CapturedDate { get; init; }
        public DateTimeOffset LastModified { get; init; }
        public TransactionStatus Status { get; init; }
        public ChargeMethod Method { get; init; }
        public SyncStatus SyncStatus { get; init; }
        public object? AdditionalData { get; init; }
    }
}