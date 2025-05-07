namespace Quivi.Infrastructure.Abstractions.Events.Data.Merchants
{
    public record OnMerchantAssociatedOperationEvent : IOperationEvent
    {
        public required EntityOperation Operation { get; init; }
        public required int MerchantId { get; init; }
        public required int UserId { get; init; }
    }
}
