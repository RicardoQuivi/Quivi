namespace Quivi.Infrastructure.Abstractions.Events.Data.ChannelProfiles
{
    public record OnChannelProfileOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}