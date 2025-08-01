namespace Quivi.Infrastructure.Abstractions.Configurations
{
    public interface IAppHostsSettings
    {
        string Background { get; }
        string OAuth { get; }
        string Backoffice { get; }
        string BackofficeApi { get; }
        string GuestsApp { get; }
    }
}
