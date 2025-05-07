namespace Quivi.Infrastructure.Abstractions.Events.Data.PosCharges
{
    public record OnPosChargeSyncedEvent : IEvent
    {
        public int Id { get; init; }
        public int ChannelId { get; init; }
        public int SessionId { get; init; }
        public int MerchantId { get; init; }
    }
}