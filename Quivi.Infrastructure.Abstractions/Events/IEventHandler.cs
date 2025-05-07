namespace Quivi.Infrastructure.Abstractions.Events
{
    public interface IEventHandler<T> where T : IEvent
    {
        Task Process(T message);
    }
}
