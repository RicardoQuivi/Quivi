namespace Quivi.Infrastructure.Abstractions.Events.Data.MenuItems
{
    public record OnMenuItemAvailabilityChangedEvent : IEvent
    {
        public int MerchantId { get; init; }
        public int ChannelProfileId { get; init; }
        public int Id { get; init; }
    }
}