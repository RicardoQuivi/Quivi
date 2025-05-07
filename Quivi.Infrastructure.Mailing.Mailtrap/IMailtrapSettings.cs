namespace Quivi.Infrastructure.Mailing.Mailtrap
{
    public interface IMailtrapSettings
    {
        string FromAddress { get; }
        string FromName { get; }
        string Host { get; }
        int Port { get; }
        string Username { get; }
        string Password { get; }
    }
}