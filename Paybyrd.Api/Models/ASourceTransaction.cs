namespace Paybyrd.Api.Models
{
    public abstract class ASourceTransaction
    {
        public required string TransactionId { get; init; }

        public required decimal Amount { get; init; }
        public required int IsoAmount { get; init; }

        public required decimal RemainingAmount { get; init; }
        public required int IsoRemainingAmount { get; init; }
    }
}
