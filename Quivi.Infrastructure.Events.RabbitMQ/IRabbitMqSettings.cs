namespace Quivi.Infrastructure.Events.RabbitMQ
{
    public interface IRabbitMqSettings
    {
        IEnumerable<string> Hosts { get; }
        string Username { get; }
        string Password { get; }
        bool RoundRobin { get; }
    }
}