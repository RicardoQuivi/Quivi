using System.Net;
using System.Net.Mail;

namespace Quivi.Infrastructure.Mailing.Smtp
{
    public class SmtpEmailService : Abstractions.Services.Mailing.IEmailService
    {
        public required string FromAddress { get; init; }
        public required string FromName { get; init; }
        public required string Host { get; init; }
        public required int Port { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }

        public Task SendAsync(Abstractions.Services.Mailing.MailMessage message) => SendAsync(message, Enumerable.Empty<Abstractions.Services.Mailing.MailAttachment>());

        public Task SendAsync(Abstractions.Services.Mailing.MailMessage message, IEnumerable<Abstractions.Services.Mailing.MailAttachment> attachments)
        {
            var smtpClient = new SmtpClient(this.Host, this.Port)
            {
                Credentials = new NetworkCredential(this.Username, this.Password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(this.FromAddress, this.FromName),
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