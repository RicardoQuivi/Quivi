namespace Quivi.Infrastructure.Abstractions.Services.Mailing
{
    public class MailMessage
    {
        public required string ToAddress { get; init; }
        public required string Body { get; init; }
        public required string Subject { get; init; }
    }
}