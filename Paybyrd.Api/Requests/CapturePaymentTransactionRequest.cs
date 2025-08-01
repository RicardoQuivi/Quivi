namespace Paybyrd.Api.Requests
{
    public class CapturePaymentTransactionRequest
    {
        public required string TransactionId { get; init; }
        public required decimal Amount { get; init; }
        public required int IsoAmount { get; init; }
    }
}