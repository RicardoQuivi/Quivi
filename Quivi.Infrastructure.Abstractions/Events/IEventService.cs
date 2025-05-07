namespace Quivi.Infrastructure.Abstractions.Events
{
    public interface IEventService : IDisposable
    {
        Task Publish<T>(T evt) where T : IEvent;
    }
}
