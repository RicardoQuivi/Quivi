namespace Quivi.Infrastructure.Abstractions.Events.Data.Channels
{
    public record OnChannelOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}