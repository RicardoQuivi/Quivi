namespace Quivi.Backoffice.Api.Requests.Users
{
    public class CreateUserRequest : ARequest
    {
        public string Email { get; init; } = string.Empty;
        public string? Password { get; init; }
    }
}
