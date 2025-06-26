namespace Quivi.Infrastructure.Mailing.Smtp
{
    public interface ISmtpSettings
    {
        string FromAddress { get; }
        string FromName { get; }
        string Host { get; }
        int Port { get; }
        string Username { get; }
        string Password { get; }
    }
}