namespace Quivi.Infrastructure.Abstractions.Events.Data.Sessions
{
    public record OnSessionOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int ChannelId { get; init; }
        public int MerchantId { get; init; }
    }
}