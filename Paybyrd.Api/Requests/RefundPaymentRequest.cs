namespace Paybyrd.Api.Requests
{
    public class RefundPaymentRequest
    {
        public required string TransactionId { get; init; }
        public required int IsoAmount { get; init; }
        public required decimal Amount { get; init; }
    }
}