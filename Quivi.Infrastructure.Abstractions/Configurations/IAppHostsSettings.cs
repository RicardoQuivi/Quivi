namespace Quivi.Infrastructure.Abstractions.Configurations
{
    public interface IAppHostsSettings
    {
        string OAuth { get; }
        string Backoffice { get; }
        string BackofficeApi { get; }
        string GuestApp { get; }
    }
}
