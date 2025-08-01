using Paybyrd.Api.Models;

namespace Paybyrd.Api.Responses
{
    public class GetPaymentResponse
    {
        public required string TransactionId { get; init; }
        public decimal Amount { get; init; }
        public required TransactionStatus Status { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? TransactionDate { get; init; }
        public DateTime? CapturedDate { get; init; }
    }
}
