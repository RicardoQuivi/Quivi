namespace Quivi.Printer.MassTransit.Configurations
{
    public interface IPrinterRabbitMqSettings
    {
        string Host { get; }
        int Port { get; }
        string Username { get; }
        string Password { get; }
    }
}