namespace Quivi.Infrastructure.Payouts.ComplyPay
{
    public interface IComplyPaySettings
    {
        bool SkipPayouts { get; }
        string? TreasuryId { get; }
        string Host { get; }
        string Email { get; }
        string Password { get; }
    }
}