namespace Quivi.Application.Configurations
{
    public class MailingSettings
    {
        public required string Provider { get; init; }
        public required string FromName { get; init; }
        public required string FromAddress { get; init; }
    }
}