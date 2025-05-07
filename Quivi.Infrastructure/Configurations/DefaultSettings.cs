using Quivi.Infrastructure.Abstractions.Configurations;

namespace Quivi.Infrastructure.Configurations
{
    public class DefaultSettings : IDefaultSettings
    {
        public required string DefaultMerchantLogo { get; init; }
    }
}
