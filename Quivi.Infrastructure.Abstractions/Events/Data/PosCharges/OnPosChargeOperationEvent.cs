namespace Quivi.Infrastructure.Abstractions.Events.Data.PosCharges
{
    public record OnPosChargeOperationEvent : IOperationEvent
    {
        public required EntityOperation Operation { get; init; }
        public required int Id { get; init; }
        public required int ChannelId { get; init; }
        public required int MerchantId { get; init; }
    }
}