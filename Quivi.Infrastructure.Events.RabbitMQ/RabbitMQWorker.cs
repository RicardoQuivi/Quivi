using Newtonsoft.Json;
using Quivi.Infrastructure.Abstractions.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Reflection;
using Quivi.Infrastructure.Abstractions.Services;

namespace Quivi.Infrastructure.Events.RabbitMQ
{
    public class RabbitMQWorker : IDisposable, IStartUpService
    {
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        };

        private readonly RabbitMQConnection _connection;
        private readonly string _appName;
        private readonly IServiceProvider _serviceProvider;
        private readonly bool _isRoundRobin;

        private readonly ConcurrentDictionary<RabbitMQConnection, IModel?> _connections;
        private readonly CancellationTokenSource _cancellationToken;

        private readonly Lazy<IEnumerable<Type>> processors;

        private IEnumerable<Type> ProcessorsTypes => processors.Value;

        public RabbitMQWorker(RabbitMQConnection connection, string appName, IServiceProvider serviceProvider, bool isRoundRobin = true)
        {
            _connection = connection;
            _connections = new ConcurrentDictionary<RabbitMQConnection, IModel?>();
            _appName = appName;
            _serviceProvider = serviceProvider;
            _isRoundRobin = isRoundRobin;
            _cancellationToken = new CancellationTokenSource();

            processors = new Lazy<IEnumerable<Type>>(() =>
            {
                return GetSafeAssembliesTypes(AppDomain.CurrentDomain, a => !a.IsDynamic)
                                .Where(type => type.IsAbstract == false)
                                .SelectMany(type => type.GetInterfaces()
                                                    .Where(y => y.IsGenericType)
                                                    .Where(y => y.GetGenericTypeDefinition().Equals(typeof(IEventHandler<>)))
                                                    .Select(y => (type, y)))
                                .GroupBy(t => t.y)
                                .Select(t => t.Key)
                                .ToList();
            });
        }

        public async Task StartUpAsync()
        {
            if (_cancellationToken.IsCancellationRequested)
                return;

            if(ProcessorsTypes.Any() == false)
                return;

            var result = _connections.AddOrUpdate(_connection, (e) => CreateChannel(e, ProcessorsTypes), (e, previousChannel) =>
            {
                if (previousChannel?.IsOpen == true)
                    return previousChannel;

                DisposeChannel(previousChannel);
                return CreateChannel(e, ProcessorsTypes);
            });

            if (_cancellationToken.IsCancellationRequested)
                return;

            await Task.Delay(5000, _cancellationToken.Token);
            await StartUpAsync();
        }

        private static void DisposeChannel(IModel? previousChannel)
        {
            if (previousChannel != null)
            {
                if (previousChannel.IsOpen)
                    previousChannel.Close();
                previousChannel.Dispose();
            }
        }

        private IModel? CreateChannel(RabbitMQConnection connection, IEnumerable<Type> processorsTypes)
        {
            try
            {
                var channel = connection.CreateChannel();

                var startProcessorMethod = typeof(RabbitMQWorker).GetMethod(nameof(StartProcessor));
                if (startProcessorMethod == null)
                    throw new Exception("This should never happen");

                foreach (var item in processorsTypes)
                {
                    var iface = item;
                    var argumentType = iface.GetGenericArguments().Single();
                    if (argumentType.IsGenericParameter)
                        continue;

                    var m = startProcessorMethod.MakeGenericMethod(argumentType);
                    m.Invoke(this, [ channel ]);
                }
                return channel;
            }
            catch (Exception)
            {
            }
            return null;
        }

        public void StartProcessor<T>(IModel channel) where T : IEvent
        {
            var exchangeName = typeof(T).FullName;
            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false);

            string queueName = _isRoundRobin ? $"{_appName}:{exchangeName}" : $"{Environment.MachineName}:{_appName}:{exchangeName}";
            channel.QueueDeclare(queueName, exclusive: false, durable: true, autoDelete: false);
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var data = JsonConvert.DeserializeObject<T>(message, _jsonSettings);
                if (data == null)
                    throw new Exception("This should never happen. No null message can be published!");

                using (var scope = _serviceProvider.CreateScope())
                {
                    List<IEventHandler<T>> processors = new List<IEventHandler<T>>();
                    var enumerableProcessors = scope.ServiceProvider.GetServices<IEventHandler<T>>();
                    if (enumerableProcessors != null)
                        processors.AddRange(enumerableProcessors);
                    else
                    {
                        var p = scope.ServiceProvider.GetService<IEventHandler<T>>();
                        if (p != null)
                            processors.Add(p);
                    }

                    foreach (var processor in processors)
                        await processor.Process(data);

                    if (consumer.Model.IsOpen == false)
                        return;
                    consumer.Model.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();

            if (_connections.TryRemove(_connection, out var channel) == false)
                return;

            DisposeChannel(channel);
        }

        private IEnumerable<Type> GetSafeAssembliesTypes(AppDomain appDomain, Func<Assembly, bool>? assemblyFilter = null)
        {
            IEnumerable<Assembly> assemblies = appDomain.GetAssemblies();

            if (assemblyFilter != null)
                assemblies = assemblies.Where(assemblyFilter);

            return assemblies.SelectMany(GetTypes);
        }

        [DebuggerStepThrough]
        private static IEnumerable<Type> GetTypes(Assembly a)
        {
            try
            {
                return a.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                try
                {
                    return a.ExportedTypes;
                }
                catch (Exception)
                {
                    return Enumerable.Empty<Type>();
                }
            }
        }
    }
}
