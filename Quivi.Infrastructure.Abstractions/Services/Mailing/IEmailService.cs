namespace Quivi.Infrastructure.Abstractions.Services.Mailing
{
    public interface IEmailService
    {
        Task SendAsync(MailMessage message);
        Task SendAsync(MailMessage message, IEnumerable<MailAttachment> attachments);
    }
}
