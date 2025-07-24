namespace Quivi.Infrastructure.Abstractions.Services.Mailing
{
    public class ConfirmEmailParameters
    {
        public required string Email { get; init; }
        public required string ConfirmUrl { get; init; }
    }
}