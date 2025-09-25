using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Quivi.Infrastructure.Events.RabbitMQ
{
    public class RabbitMQWorker : IDisposable, IStartUpService
    {
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        };

        private readonly RabbitMQConnection connection;
        private readonly string appName;
        private readonly IServiceProvider serviceProvider;
        private readonly bool isRoundRobin;

        private readonly ConcurrentDictionary<RabbitMQConnection, IModel?> connections;
        private readonly CancellationTokenSource cancellationToken;

        private readonly Lazy<IEnumerable<Type>> processors;

        private IEnumerable<Type> ProcessorsTypes => processors.Value;

        public RabbitMQWorker(RabbitMQConnection connection, string appName, IServiceProvider serviceProvider, bool isRoundRobin = true)
        {
            this.connection = connection;
            connections = new ConcurrentDictionary<RabbitMQConnection, IModel?>();
            this.appName = appName;
            this.serviceProvider = serviceProvider;
            this.isRoundRobin = isRoundRobin;
            cancellationToken = new CancellationTokenSource();

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
            if (cancellationToken.IsCancellationRequested)
                return;

            if (ProcessorsTypes.Any() == false)
                return;

            var result = connections.AddOrUpdate(connection, (e) => CreateChannel(e, ProcessorsTypes), (e, previousChannel) =>
            {
                if (previousChannel?.IsOpen == true)
                    return previousChannel;

                DisposeChannel(previousChannel);
                return CreateChannel(e, ProcessorsTypes);
            });

            if (cancellationToken.IsCancellationRequested)
                return;

            await Task.Delay(5000, cancellationToken.Token);
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
                    m.Invoke(this, [channel]);
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
            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: true, autoDelete: false);

            string queueName = isRoundRobin ? $"{appName}:{exchangeName}" : $"{Environment.MachineName}:{appName}:{exchangeName}";

            bool isDurable = isRoundRobin;
            bool isExclusive = !isRoundRobin;
            bool isAutoDelete = !isRoundRobin;

            channel.QueueDeclare(queue: queueName, durable: isDurable, exclusive: isExclusive, autoDelete: isAutoDelete);
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var data = JsonConvert.DeserializeObject<T>(message, jsonSettings);
                if (data == null)
                    throw new Exception("This should never happen. No null message can be published!");

                await using (var scope = serviceProvider.CreateAsyncScope())
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
            cancellationToken.Cancel();

            if (connections.TryRemove(connection, out var channel) == false)
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
