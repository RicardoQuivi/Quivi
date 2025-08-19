using System.Security.Cryptography.X509Certificates;

namespace Quivi.Infrastructure.Abstractions.Configurations
{
    public interface IJwtSettings
    {
        string Issuer { get; }
        IEnumerable<string> Audiences { get; }
        string Secret { get; }
        X509Certificate2 SigningCertificate { get; }
        X509Certificate2 EncryptionCertificate { get; }
        TimeSpan ExpireTimeSpan { get; }
        TimeSpan RefreshTokenExpireTimeSpan { get; }
    }
}