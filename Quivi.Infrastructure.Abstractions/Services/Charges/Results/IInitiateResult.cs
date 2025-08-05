namespace Quivi.Infrastructure.Abstractions.Services.Charges.Results
{
    public interface IInitiateResult
    {
        public string? GatewayId { get; }
        public bool CaptureStarted { get; }
    }
}