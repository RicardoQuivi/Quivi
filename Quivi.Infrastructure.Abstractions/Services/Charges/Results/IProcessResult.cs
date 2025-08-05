namespace Quivi.Infrastructure.Abstractions.Services.Charges.Results
{
    public interface IProcessResult
    {
        public string? GatewayId { get; }
        public string? ChallengeUrl { get; }
    }
}