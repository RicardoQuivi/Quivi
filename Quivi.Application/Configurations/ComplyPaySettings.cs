using Quivi.Infrastructure.Payouts.ComplyPay;

namespace Quivi.Application.Configurations
{
    public class ComplyPaySettings : IComplyPaySettings
    {
        public bool SkipPayouts { get; init; }
        public string? TreasuryId { get; init; }
        public required string Host { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}