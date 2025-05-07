using Quivi.Infrastructure.Mailing.Mailtrap;

namespace Quivi.Application.Configurations
{
    public class MailtrapSettings : IMailtrapSettings
    {
        public required string FromAddress { get; init; }
        public required string FromName { get; init; }
        public required string Host { get; init; }
        public required int Port { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }
    }
}
