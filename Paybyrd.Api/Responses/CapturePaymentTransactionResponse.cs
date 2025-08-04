using Paybyrd.Api.Models;

namespace Paybyrd.Api.Responses
{
    public class CapturedSourceTransaction : ASourceTransaction
    {
        public required decimal CapturedAmount { get; init; }
        public required int IsoCapturedAmount { get; init; }
    }

    public class CapturePaymentTransactionResponse
    {
        public required string TransactionId { get; init; }
        public decimal Amount { get; init; }
        public required int IsoAmount { get; init; }
        public required string Code { get; init; }
        public required string Description { get; init; }
        public required CapturedSourceTransaction SourceTransaction { get; init; }
    }
}