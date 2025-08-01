using Quivi.Infrastructure.Abstractions.Configurations;

namespace Quivi.Infrastructure.Configurations
{
    public class AppHostsSettings : IAppHostsSettings
    {
        public required string Background { get; set; }
        public required string OAuth { get; set; }
        public required string Backoffice { get; set; }
        public required string BackofficeApi { get; set; }
        public required string GuestsApp { get; set; }
    }
}
