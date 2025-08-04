using Paybyrd.Api.Models;

namespace Paybyrd.Api.Responses
{
    public class RefundedSourceTransaction : ASourceTransaction
    {
        public required decimal RefundedAmount { get; init; }
        public required int IsoRefundedAmount { get; init; }
    }

    public class RefundPaymentResponse
    {
        public required string TransactionId { get; init; }
        public required int IsoAmount { get; init; }
        public required decimal Amount { get; init; }
        public required string Code { get; init; }
        public required string Description { get; init; }
        public required RefundedSourceTransaction SourceTransaction { get; init; }
    }
}