namespace Quivi.Backoffice.Api.Requests.Auth
{
    public class JwtAuthRequest : ARequest
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}
