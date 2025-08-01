namespace Paybyrd.Api.Responses
{
    public class SourceTransaction
    {
        public required string TransactionId { get; init; }

        public required decimal Amount { get; init; }
        public required int IsoAmount { get; init; }

        public required decimal CapturedAmount { get; init; }
        public required int IsoCapturedAmount { get; init; }

        public required decimal RemainingAmount { get; init; }
        public required int IsoRemainingAmount { get; init; }
    }

    public class CapturePaymentTransactionResponse
    {
        public required string TransactionId { get; init; }
        public decimal Amount { get; init; }
        public required int IsoAmount { get; init; }
        public required string Code { get; init; }
        public required string Description { get; init; }
        public required SourceTransaction SourceTransaction { get; init; }
    }
}