using Newtonsoft.Json;
using Quivi.Infrastructure.Abstractions.Events;
using RabbitMQ.Client;
using System.Text;

namespace Quivi.Infrastructure.Events.RabbitMQ
{
    public class RabbitMQEventService : IEventService, IDisposable
    {
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        };
        private Lazy<IModel> channel;
        private IModel Channel => channel.Value;

        public RabbitMQEventService(RabbitMQConnection connection)
        {
            channel = new Lazy<IModel>(() =>
            {
                var channel = connection.CreateChannel();
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                return channel;
            });
        }

        ~RabbitMQEventService()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (channel?.IsValueCreated == true)
            {
                if (channel.Value.IsOpen)
                    channel.Value.Close();
                channel.Value.Dispose();
            }
        }

        public Task Publish(IEvent evt)
        {
            var exchangeName = evt.GetType().FullName;
            Console.WriteLine($"Trying to publish into {exchangeName}");
            Channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false);
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evt, jsonSettings));
            Channel.BasicPublish(exchange: exchangeName, routingKey: "", basicProperties: null, body: body);
            return Task.CompletedTask;
        }
    }
}