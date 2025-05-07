using Quivi.Infrastructure.Abstractions.Configurations;

namespace Quivi.Infrastructure.Configurations
{
    public class AppHostsSettings : IAppHostsSettings
    {
        public required string OAuth { get; set; }
        public required string Backoffice { get; set; }
        public required string BackofficeApi { get; set; }
        public required string GuestApp { get; set; }
    }
}
