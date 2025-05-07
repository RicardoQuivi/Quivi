namespace Quivi.Infrastructure.Abstractions.Configurations
{
    public interface IJwtSettings
    {
        string Issuer { get; }
        IEnumerable<string> Audiences { get; }
        string Secret { get; }
        IJwtCertificate Certificate { get; }
        TimeSpan ExpireTimeSpan { get; }
        TimeSpan RefreshTokenExpireTimeSpan { get; }
    }

    public interface IJwtCertificate
    {
        string Base64 { get; }
        string Password { get; }
    }
}
