namespace Quivi.Infrastructure.Abstractions.Services.Mailing
{
    public class ForgotPasswordParameters
    {
        public required string Email { get; init; }
        public required string ResetPasswordUrl { get; init; }
    }
}