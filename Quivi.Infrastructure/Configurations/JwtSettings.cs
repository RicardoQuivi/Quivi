
using Quivi.Infrastructure.Abstractions.Configurations;

namespace Quivi.Infrastructure.Configurations
{
    public class JwtSettings : IJwtSettings
    {
        public required string Issuer { get; init; }
        public required IEnumerable<string> Audiences { get; init; }
        public required string Secret { get; init; }
        public required JwtCertificate Certificate { get; init; }
        TimeSpan IJwtSettings.ExpireTimeSpan => TimeSpan.Parse(ExpireTimeSpan);
        TimeSpan IJwtSettings.RefreshTokenExpireTimeSpan => TimeSpan.Parse(RefreshTokenExpireTimeSpan);

        public required string ExpireTimeSpan { get; init; }
        public required string RefreshTokenExpireTimeSpan { get; init; }

        IJwtCertificate IJwtSettings.Certificate => Certificate;

        public class JwtCertificate : IJwtCertificate
        {
            public required string Base64 { get; init; }

            public required string Password { get; init; }
        }
    }
}
