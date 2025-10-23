namespace ComplyPay.Dtos.Requests
{
    public class Payer
    {
        public string? Id { get; init; }
        public AccountType Type { get; init; }
    }

    public class Payee
    {
        public required string Id { get; init; }
    }

    public class CreatePaymentRequest
    {
        public string? IdempotencyKey { get; init; }
        public int Amount { get; init; }
        public required string Currency { get; init; }
        public required Payer Payer { get; init; }
        public required Payee Payee { get; init; }
        public string? Memo { get; init; }
        public string? Description { get; init; }
        public PaymentFlowType PaymentFlowType { get; init; }
        public IReadOnlyDictionary<string, string>? Metadata { get; init; }
    }
}