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
        private Lazy<IModel> _channel;
        private IModel Channel => _channel.Value;

        public RabbitMQEventService(RabbitMQConnection connection)
        {
            _channel = new Lazy<IModel>(() =>
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
            if (_channel?.IsValueCreated == true)
            {
                if (_channel.Value.IsOpen)
                    _channel.Value.Close();
                _channel.Value.Dispose();
            }
        }

        public Task Publish<T>(T data) where T : IEvent
        {
            var exchangeName = typeof(T).FullName;
            Channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false);
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, jsonSettings));
            Channel.BasicPublish(exchange: exchangeName, routingKey: "", basicProperties: null, body: body);
            return Task.CompletedTask;
        }
    }
}
