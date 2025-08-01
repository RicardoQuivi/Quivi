namespace Quivi.Infrastructure.Abstractions.Services.Charges
{
    public class CreditCardResult
    {
        public string? ChallengeUrl { get; init; }
    }

    public class ProcessResult
    {
        public required string GatewayTransactionId { get; init; }
        public CreditCardResult? CreditCard { get; init; }
    }
}