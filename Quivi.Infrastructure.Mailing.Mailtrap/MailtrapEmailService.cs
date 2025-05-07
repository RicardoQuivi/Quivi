using Quivi.Infrastructure.Abstractions.Services.Mailing;
using System.Net.Mail;
using System.Net;

namespace Quivi.Infrastructure.Mailing.Mailtrap
{
    public class MailtrapEmailService : IEmailService
    {
        private readonly IMailtrapSettings settings;

        public MailtrapEmailService(IMailtrapSettings settings)
        {
            this.settings = settings;
        }

        public Task SendAsync(Abstractions.Services.Mailing.MailMessage message) => SendAsync(message, Enumerable.Empty<MailAttachment>());

        public Task SendAsync(Abstractions.Services.Mailing.MailMessage message, IEnumerable<MailAttachment> attachments)
        {
            var mailMessage = new System.Net.Mail.MailMessage();
            mailMessage.To.Add(message.ToAddress);
            mailMessage.Body = message.Body;
            mailMessage.Subject = message.Subject;
            mailMessage.IsBodyHtml = true;

            var client = new SmtpClient(settings.Host, settings.Port)
            {
                Credentials = new NetworkCredential(settings.Username, settings.Password),
                EnableSsl = true,
            };

            var @from = new MailAddress(settings.FromAddress, settings.FromName);
            mailMessage.From = @from;

            foreach (var attachment in attachments.Where(a => a.Bytes.Length > 0))
            {
                var ms = new MemoryStream(attachment.Bytes);
                mailMessage.Attachments.Add(new Attachment(ms, attachment.Name));
            }

            return client.SendMailAsync(mailMessage);
        }
    }
}
