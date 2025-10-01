namespace Quivi.Guests.Api.Dtos.Requests.Users
{
    public class ResetUserPasswordRequest : ARequest
    {
        public string Email { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}