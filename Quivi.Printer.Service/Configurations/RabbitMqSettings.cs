namespace Quivi.Printer.Service.Configurations
{
    public class RabbitMqSettings
    {
        public required string Host { get; init; }
        public required ushort Port { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }
        public required string VirtualHost { get; init; }
    }
}
