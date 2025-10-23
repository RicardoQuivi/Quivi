namespace ComplyPay.Dtos.Responses
{
    public class JwtRefreshResponse
    {
        public required string StatusMessage { get; init; }
        public required string Token { get; init; }
        public required string RefreshToken { get; init; }
    }
}