namespace Quivi.Infrastructure.Abstractions.Services.Charges.Results
{
    public interface IStatusResult
    {
        string? GatewayId { get; }
        PaymentStatus Status { get; }
    }
}