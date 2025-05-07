namespace Quivi.Backoffice.Api.Requests.Users
{
    public class ForgotUserPasswordRequest : ARequest
    {
        public string Email { get; init; } = string.Empty;
    }
}
