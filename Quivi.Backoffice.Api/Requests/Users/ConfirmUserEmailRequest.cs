namespace Quivi.Backoffice.Api.Requests.Users
{
    public class ConfirmUserEmailRequest : ARequest
    {
        public string Email { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
    }
}