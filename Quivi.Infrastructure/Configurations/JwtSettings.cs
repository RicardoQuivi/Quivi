using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Quivi.Infrastructure.Abstractions.Configurations;
using System.Security.Cryptography.X509Certificates;

namespace Quivi.Infrastructure.Configurations
{
    public class JwtSettings : IJwtSettings
    {
        public required string Issuer { get; init; }
        public required IEnumerable<string> Audiences { get; init; }
        public required string Secret { get; init; }
        TimeSpan IJwtSettings.ExpireTimeSpan => TimeSpan.Parse(ExpireTimeSpan);
        TimeSpan IJwtSettings.RefreshTokenExpireTimeSpan => TimeSpan.Parse(RefreshTokenExpireTimeSpan);
        public required string ExpireTimeSpan { get; init; }
        public required string RefreshTokenExpireTimeSpan { get; init; }


        X509Certificate2 IJwtSettings.SigningCertificate
        {
            get
            {
                if (signingCertificate != null)
                    return signingCertificate;

                if (AzureCertificate != null)
                {
                    var credential = new ClientSecretCredential(AzureCertificate.TenantId, AzureCertificate.ClientId, AzureCertificate.ClientSecret);
                    var certClient = new CertificateClient(new Uri(AzureCertificate.VaultUri), credential);
                    var cert = certClient.DownloadCertificate(AzureCertificate.EncryptionName).Value;
                    signingCertificate = cert;
                    return signingCertificate;
                }

                if (SigningCertificate != null)
                {
                    signingCertificate = new X509Certificate2(Convert.FromBase64String(SigningCertificate.Base64), SigningCertificate.Password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                    return signingCertificate;
                }

                throw new Exception("Invalid Jwt Certificate Configuration");
            }
        }
        X509Certificate2 IJwtSettings.EncryptionCertificate
        {
            get
            {
                if (encryptionCertificate != null)
                    return encryptionCertificate;

                if (AzureCertificate != null)
                {
                    var credential = new ClientSecretCredential(AzureCertificate.TenantId, AzureCertificate.ClientId, AzureCertificate.ClientSecret);
                    var certClient = new CertificateClient(new Uri(AzureCertificate.VaultUri), credential);
                    var cert = certClient.DownloadCertificate(AzureCertificate.EncryptionName).Value;
                    encryptionCertificate = cert;
                    return encryptionCertificate;
                }

                if (EncryptionCertificate != null)
                {
                    encryptionCertificate = new X509Certificate2(Convert.FromBase64String(EncryptionCertificate.Base64), EncryptionCertificate.Password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                    return encryptionCertificate;
                }

                throw new Exception("Invalid Jwt Certificate Configuration");
            }
        }
        private X509Certificate2? signingCertificate;
        private X509Certificate2? encryptionCertificate;

        public JwtCertificate? SigningCertificate { get; init; }
        public JwtCertificate? EncryptionCertificate { get; init; }
        public AzureCertificateSettings? AzureCertificate { get; init; }

        public class JwtCertificate
        {
            public required string Base64 { get; init; }
            public required string Password { get; init; }
        }

        public class AzureCertificateSettings
        {
            public required string VaultUri { get; init; }
            public required string TenantId { get; init; }
            public required string ClientId { get; init; }
            public required string ClientSecret { get; init; }
            public required string SigningName { get; init; }
            public required string EncryptionName { get; init; }
        }
    }
}