using Paybyrd.Api.Models;

namespace Paybyrd.Api.Requests
{
    public class CreatePaymentCard
    {
        public required string TokenId { get; init; }
    }

    public class CreatePaymentRequest
    {
        public PaymentType Type { get; init; }
        public decimal Amount { get; init; }
        public Currency Currency { get; init; }
        public required string OrderRef { get; init; }
        public bool? IsPreAuth { get; init; }
        public string? RedirectUrl { get; init; }
        public CreatePaymentCard? Card { get; init; }
        public string? TerminalSerialNumber { get; init; }
    }
}