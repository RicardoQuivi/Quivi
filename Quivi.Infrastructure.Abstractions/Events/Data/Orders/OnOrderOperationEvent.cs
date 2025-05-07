namespace Quivi.Infrastructure.Abstractions.Events.Data.Orders
{
    public record OnOrderOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int ChannelId { get; init; }
        public int MerchantId { get; init; }
    }
}