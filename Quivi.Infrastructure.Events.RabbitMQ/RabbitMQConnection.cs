using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

namespace Quivi.Infrastructure.Events.RabbitMQ
{
    public class RabbitMQConnection : IDisposable
    {
        private readonly int _maxOpenChannels;
        private readonly ConnectionFactory _connectionFactory;
        private readonly object _lock;
        private readonly IList<AmqpTcpEndpoint> _endpoints;
        private readonly RabbitMQPool _objectPool;
        private bool _shouldClose = false;

        private IConnection? Connection { get; set; }

        public RabbitMQConnection(IRabbitMqSettings rabbitMqSettings, int maxOpenChannels = 200)
        {
            _maxOpenChannels = maxOpenChannels;
            _endpoints = rabbitMqSettings.Hosts.Select(s =>
            {
                var split = s.Split(':');
                var hostname = split[0];
                int port = -1;
                if (split.Length > 1 && int.TryParse(split[1], out int parsedPort))
                    port = parsedPort;
                return new AmqpTcpEndpoint(hostname, port);
            }).ToList();
            _connectionFactory = new ConnectionFactory
            {
                UserName = rabbitMqSettings.Username,
                Password = rabbitMqSettings.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            };

            _lock = new object();
            _objectPool = new RabbitMQPool(this, maxOpenChannels);
        }

        ~RabbitMQConnection() => Dispose();

        private void StartConnection()
        {
            if (_shouldClose)
                return;

            lock (_lock)
            {
                if (Connection?.IsOpen == true)
                    return;

                if (Connection != null)
                {
                    _objectPool.Dispose();
                    Connection.Dispose();
                }

                Connection = _connectionFactory.CreateConnection(_endpoints);
                Connection.ConnectionShutdown += Connection_ConnectionShutdown;
            }

            if (Connection?.IsOpen != true)
                return;

            //Pre-create connections
            List<ChannelWrapper> channels = Enumerable.Repeat(1, _maxOpenChannels).Select(s => _objectPool.Get()).ToList();
            foreach (var channel in channels)
                _objectPool.Return(channel);
        }

        private void Connection_ConnectionShutdown(object? sender, ShutdownEventArgs e) => StartConnection();

        public void Dispose()
        {
            _shouldClose = true;
            lock (_lock)
            {
                if (Connection != null)
                {
                    _objectPool.Dispose();
                    if (Connection.IsOpen)
                        Connection.Close();
                    Connection.Dispose();
                    Connection = null;
                }
            }
        }

        public IModel CreateChannel()
        {
            StartConnection();
            return _objectPool.Get();
        }


        private class RabbitMQPool : ObjectPool<ChannelWrapper>, IDisposable
        {
            private readonly RabbitMQConnection _connection;
            private readonly int _maxCapacity;
            private int _numItems;

            private protected readonly ConcurrentQueue<ChannelWrapper> _items = new ConcurrentQueue<ChannelWrapper>();

            public RabbitMQPool(RabbitMQConnection connection, int maximumRetained)
            {
                _connection = connection;
                _maxCapacity = maximumRetained;
            }

            public override ChannelWrapper Get()
            {
                if (_items.TryDequeue(out var item))
                {
                    Interlocked.Decrement(ref _numItems);
                    item.OnTakenFromPool();
                    return item;
                }
                return Create();
            }

            public override void Return(ChannelWrapper obj)
            {
                var result = ReturnCore(obj);
                if (result)
                    obj.OnReturnedToPool();
                else
                    obj.OnDiscardedFromPool();
            }

            private bool ReturnCore(ChannelWrapper obj)
            {
                if (ChannelIsFit(obj) == false)
                    return false;

                if (Interlocked.Increment(ref _numItems) <= _maxCapacity)
                {
                    _items.Enqueue(obj);
                    return true;
                }

                Interlocked.Decrement(ref _numItems);
                return false;
            }

            private ChannelWrapper Create()
            {
                var channel = new ChannelWrapper(_connection.Connection!.CreateModel(), this);
                return channel;
            }

            private bool ChannelIsFit(ChannelWrapper obj) => obj.IsOpen;

            public void Dispose()
            {
                while (_items.TryDequeue(out var item))
                {
                    Interlocked.Decrement(ref _numItems);
                    item.OnTakenFromPool();
                }
            }
        }


        private class ChannelWrapper : IModel
        {
            enum PoolState
            {
                AddingToPool,
                InPool,
                RemovedFromPool,
            }

            private IModel Model { get; }
            private RabbitMQPool ObjectPool { get; }
            private object lck = new object();
            private PoolState state = PoolState.RemovedFromPool;

            public int ChannelNumber => Model.ChannelNumber;
            public ShutdownEventArgs CloseReason => Model.CloseReason;
            public bool IsClosed => Model.IsClosed;
            public bool IsOpen => Model.IsOpen;
            public ulong NextPublishSeqNo => Model.NextPublishSeqNo;
            public string CurrentQueue => Model.CurrentQueue;

            public ChannelWrapper(IModel model, RabbitMQPool objectPool)
            {
                Model = model;
                ObjectPool = objectPool;
            }

            public void OnReturnedToPool() => state = PoolState.InPool;
            public void OnTakenFromPool() => state = PoolState.RemovedFromPool;
            public void OnDiscardedFromPool()
            {
                state = PoolState.RemovedFromPool;
                if (Model.IsOpen)
                    Model.Close();
                Model.Dispose();
            }

            public void Close()
            {
                lock (lck)
                {
                    if (state == PoolState.InPool)
                        return;

                    if (state == PoolState.AddingToPool)
                        return;

                    state = PoolState.AddingToPool;
                    ObjectPool.Return(this);
                }
            }
            public void Dispose() => this.Close();

            public IBasicConsumer DefaultConsumer
            {
                get => Model.DefaultConsumer;
                set => Model.DefaultConsumer = value;
            }
            public TimeSpan ContinuationTimeout
            {
                get => Model.ContinuationTimeout;
                set => Model.ContinuationTimeout = value;
            }
            public event EventHandler<BasicAckEventArgs> BasicAcks
            {
                add => Model.BasicAcks += value;
                remove => Model.BasicAcks -= value;
            }
            public event EventHandler<BasicNackEventArgs> BasicNacks
            {
                add => Model.BasicNacks += value;
                remove => Model.BasicNacks -= value;
            }
            public event EventHandler<EventArgs> BasicRecoverOk
            {
                add => Model.BasicRecoverOk += value;
                remove => Model.BasicRecoverOk -= value;
            }
            public event EventHandler<BasicReturnEventArgs> BasicReturn
            {
                add => Model.BasicReturn += value;
                remove => Model.BasicReturn -= value;
            }
            public event EventHandler<CallbackExceptionEventArgs> CallbackException
            {
                add => Model.CallbackException += value;
                remove => Model.CallbackException -= value;
            }
            public event EventHandler<FlowControlEventArgs> FlowControl
            {
                add => Model.FlowControl += value;
                remove => Model.FlowControl -= value;
            }
            public event EventHandler<ShutdownEventArgs> ModelShutdown
            {
                add => Model.ModelShutdown += value;
                remove => Model.ModelShutdown -= value;
            }

            public void Close(ushort replyCode, string replyText) => Model.Close(replyCode, replyText);
            public void Abort() => Model.Abort();
            public void Abort(ushort replyCode, string replyText) => Model.Abort(replyCode, replyText);
            public void BasicAck(ulong deliveryTag, bool multiple) => Model.BasicAck(deliveryTag, multiple);
            public void BasicCancel(string consumerTag) => Model.BasicCancel(consumerTag);
            public void BasicCancelNoWait(string consumerTag) => Model.BasicCancelNoWait(consumerTag);
            public string BasicConsume(string queue, bool autoAck, string consumerTag, bool noLocal, bool exclusive, IDictionary<string, object> arguments, IBasicConsumer consumer) => Model.BasicConsume(queue, autoAck, consumerTag, noLocal, exclusive, arguments, consumer);
            public BasicGetResult BasicGet(string queue, bool autoAck) => Model.BasicGet(queue, autoAck);
            public void BasicNack(ulong deliveryTag, bool multiple, bool requeue) => Model.BasicNack(deliveryTag, multiple, requeue);
            public void BasicPublish(string exchange, string routingKey, bool mandatory, IBasicProperties basicProperties, ReadOnlyMemory<byte> body) => Model.BasicPublish(exchange, routingKey, mandatory, basicProperties, body);
            public void BasicQos(uint prefetchSize, ushort prefetchCount, bool global) => Model.BasicQos(prefetchSize, prefetchCount, global);
            public void BasicRecover(bool requeue) => Model.BasicRecover(requeue);
            public void BasicRecoverAsync(bool requeue) => Model.BasicRecoverAsync(requeue);
            public void BasicReject(ulong deliveryTag, bool requeue) => Model.BasicReject(deliveryTag, requeue);
            public void ConfirmSelect() => Model.ConfirmSelect();
            public uint ConsumerCount(string queue) => Model.ConsumerCount(queue);
            public IBasicProperties CreateBasicProperties() => Model.CreateBasicProperties();
            public IBasicPublishBatch CreateBasicPublishBatch() => Model.CreateBasicPublishBatch();
            public void ExchangeBind(string destination, string source, string routingKey, IDictionary<string, object> arguments) => Model.ExchangeBind(destination, source, routingKey, arguments);
            public void ExchangeBindNoWait(string destination, string source, string routingKey, IDictionary<string, object> arguments) => Model.ExchangeBindNoWait(destination, source, routingKey, arguments);
            public void ExchangeDeclare(string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments) => Model.ExchangeDeclare(exchange, type, durable, autoDelete, arguments);
            public void ExchangeDeclareNoWait(string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments) => Model.ExchangeDeclareNoWait(exchange, type, durable, autoDelete, arguments);
            public void ExchangeDeclarePassive(string exchange) => Model.ExchangeDeclarePassive(exchange);
            public void ExchangeDelete(string exchange, bool ifUnused) => Model.ExchangeDelete(exchange, ifUnused);
            public void ExchangeDeleteNoWait(string exchange, bool ifUnused) => Model.ExchangeDeleteNoWait(exchange, ifUnused);
            public void ExchangeUnbind(string destination, string source, string routingKey, IDictionary<string, object> arguments) => Model.ExchangeUnbind(destination, source, routingKey, arguments);
            public void ExchangeUnbindNoWait(string destination, string source, string routingKey, IDictionary<string, object> arguments) => Model.ExchangeUnbindNoWait(destination, source, routingKey, arguments);
            public uint MessageCount(string queue) => Model.MessageCount(queue);
            public void QueueBind(string queue, string exchange, string routingKey, IDictionary<string, object> arguments) => Model.QueueBind(queue, exchange, routingKey, arguments);
            public void QueueBindNoWait(string queue, string exchange, string routingKey, IDictionary<string, object> arguments) => Model.QueueBindNoWait(queue, exchange, routingKey, arguments);
            public QueueDeclareOk QueueDeclare(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments) => Model.QueueDeclare(queue, durable, exclusive, autoDelete, arguments);
            public void QueueDeclareNoWait(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments) => Model.QueueDeclareNoWait(queue, durable, exclusive, autoDelete, arguments);
            public QueueDeclareOk QueueDeclarePassive(string queue) => Model.QueueDeclarePassive(queue);
            public uint QueueDelete(string queue, bool ifUnused, bool ifEmpty) => Model.QueueDelete(queue, ifUnused, ifEmpty);
            public void QueueDeleteNoWait(string queue, bool ifUnused, bool ifEmpty) => Model.QueueDeleteNoWait(queue, ifUnused, ifEmpty);
            public uint QueuePurge(string queue) => Model.QueuePurge(queue);
            public void QueueUnbind(string queue, string exchange, string routingKey, IDictionary<string, object> arguments) => Model.QueueUnbind(queue, exchange, routingKey, arguments);
            public void TxCommit() => Model.TxCommit();
            public void TxRollback() => Model.TxRollback();
            public void TxSelect() => Model.TxSelect();
            public bool WaitForConfirms() => Model.WaitForConfirms();
            public bool WaitForConfirms(TimeSpan timeout) => Model.WaitForConfirms(timeout);
            public bool WaitForConfirms(TimeSpan timeout, out bool timedOut) => Model.WaitForConfirms(timeout, out timedOut);
            public void WaitForConfirmsOrDie() => Model.WaitForConfirmsOrDie();
            public void WaitForConfirmsOrDie(TimeSpan timeout) => Model.WaitForConfirmsOrDie(timeout);
        }
    }
}
