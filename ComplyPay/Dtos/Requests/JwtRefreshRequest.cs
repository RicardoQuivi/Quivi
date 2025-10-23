namespace ComplyPay.Dtos.Requests
{
    public class JwtRefreshRequest
    {
        public required string RefreshToken { get; init; }
    }
}