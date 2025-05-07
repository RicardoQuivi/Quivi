namespace Quivi.Backoffice.Api.Responses.Auth
{
    public class JwtAuthResponse : AResponse
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
    }
}
