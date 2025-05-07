namespace Quivi.Infrastructure.Abstractions.Events.Data
{
    public enum EntityOperation
    {
        Create = 1,
        Update = 2,
        Delete = 3,
    }

    public interface IOperationEvent : IEvent
    {
        EntityOperation Operation { get; }
    }
}