namespace ComplyPay.Dtos.Requests
{
    public class AuthenticateJwtRequest
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}
