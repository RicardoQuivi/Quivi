namespace Quivi.Guests.Api.Dtos.Requests.Users
{
    public class CreateUserRequest : ARequest
    {
        public required string Name { get; init; } = string.Empty;
        public string? VatNumber { get; init; }
        public string? PhoneNumber { get; init; }
        public required string Email { get; init; } = string.Empty;
        public required string Password { get; init; } = string.Empty;
    }
}