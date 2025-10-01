namespace Quivi.Guests.Api.Dtos.Requests.Users
{
    public class ForgotUserPasswordRequest : ARequest
    {
        public string Email { get; init; } = string.Empty;
    }
}