namespace Quivi.Infrastructure.Abstractions.Events.Data.Orders
{
    public record OnOrderScheduledEvent : IEvent
    {
        public int Id { get; init; }
        public int ChannelId { get; init; }
        public int MerchantId { get; init; }
    }
}