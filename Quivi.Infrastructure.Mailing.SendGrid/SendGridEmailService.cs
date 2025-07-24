using Quivi.Infrastructure.Abstractions.Services.Mailing;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Quivi.Infrastructure.Mailing.SendGrid
{
    public class SendGridEmailService : IEmailService
    {
        public required string ApiKey { get; init; }
        public required string FromAddress { get; init; }
        public required string FromName { get; init; }

        public Task SendAsync(MailMessage message) => SendAsync(message, Enumerable.Empty<MailAttachment>());

        public async Task SendAsync(MailMessage message, IEnumerable<MailAttachment> attachments)
        {
            var options = new SendGridClientOptions
            {
                ApiKey = this.ApiKey
            };
            var client = new SendGridClient(options);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(this.FromAddress, this.FromName),
                Subject = message.Subject,
                HtmlContent = message.Body,
            };
            msg.AddTo(new EmailAddress(message.ToAddress));
            foreach (var attachment in attachments.Where(a => a.Bytes.Length > 0))
            {
                var ms = new MemoryStream(attachment.Bytes);
                await msg.AddAttachmentAsync(attachment.Name, ms);
            }
            await client.SendEmailAsync(msg);
        }
    }
}