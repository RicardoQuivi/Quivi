namespace Quivi.Infrastructure.Abstractions.Events
{
    public interface IEventService : IDisposable
    {
        Task Publish(IEvent evt);
    }
}
