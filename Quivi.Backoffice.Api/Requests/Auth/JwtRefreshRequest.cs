namespace Quivi.Backoffice.Api.Requests.Auth
{
    public class JwtRefreshRequest : ARequest
    {
        public string? MerchantId { get; init; }
        public required string RefreshToken { get; init; }
    }
}
