using Quivi.Infrastructure.Events.RabbitMQ;

namespace Quivi.Application.Configurations
{
    public class RabbitMqSettings : IRabbitMqSettings
    {
        public required IEnumerable<string> Hosts { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public bool RoundRobin { get; set; }
    }
}