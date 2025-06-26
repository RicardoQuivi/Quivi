using System.Net;
using System.Net.Mail;

namespace Quivi.Infrastructure.Mailing.Smtp
{
    public class SmtpEmailService : Abstractions.Services.Mailing.IEmailService
    {
        private readonly ISmtpSettings settings;

        public SmtpEmailService(ISmtpSettings settings)
        {
            this.settings = settings;
        }

        public Task SendAsync(Abstractions.Services.Mailing.MailMessage message) => SendAsync(message, Enumerable.Empty<Abstractions.Services.Mailing.MailAttachment>());

        public Task SendAsync(Abstractions.Services.Mailing.MailMessage message, IEnumerable<Abstractions.Services.Mailing.MailAttachment> attachments)
        {
            var smtpClient = new SmtpClient(settings.Host, settings.Port)
            {
                Credentials = new NetworkCredential(settings.Username, settings.Password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(settings.FromAddress, settings.FromName),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(message.ToAddress);

            foreach (var attachment in attachments.Where(a => a.Bytes.Length > 0))
            {
                var ms = new MemoryStream(attachment.Bytes);
                mailMessage.Attachments.Add(new Attachment(ms, attachment.Name));
            }

            return smtpClient.SendMailAsync(mailMessage);
        }
    }
}