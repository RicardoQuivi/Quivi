using Paybyrd.Api.Models;

namespace Paybyrd.Api.Responses
{
    public class CreatePaymentResponse
    {
        public required string TransactionId { get; init; }
        public PaymentType Type { get; init; }
        public Currency Currency { get; init; }
        public required string OrderRef { get; init; }
        public required string Brand { get; init; }
        public string? Fingerprint { get; init; }
        public decimal Amount { get; init; }
        public bool IsPreAuth { get; init; }
        public CardDetails? Card { get; init; }
        public required string Code { get; init; }
        public required string Description { get; init; }
        public ThreeDSecure? ThreeDSecure { get; init; }
        public CreatePaymentAction? Action { get; init; }
    }
}