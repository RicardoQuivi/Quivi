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
            try
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
                    using var ms = new MemoryStream(attachment.Bytes);
                    await msg.AddAttachmentAsync(attachment.Name, ms);
                }

                Console.WriteLine($"Sending email to: {message.ToAddress}");
                Console.WriteLine($"From: {this.FromAddress}");
                Console.WriteLine($"Subject: {message.Subject}");

                var response = await client.SendEmailAsync(msg);
                var responseBody = await response.Body.ReadAsStringAsync();

                Console.WriteLine($"SendGrid response status: {response.StatusCode}");
                Console.WriteLine($"SendGrid response body: {responseBody}");
            }
            catch (Exception ex)
            {
                // Log to standard error
                Console.Error.WriteLine($"[SendEmail ERROR] {ex.GetType().Name}: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
            }
        }
    }
}