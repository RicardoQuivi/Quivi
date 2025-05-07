namespace Quivi.Infrastructure.Abstractions.Events.Data.Merchants
{
    public record OnMerchantOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int? ParentId { get; init; }
    }
}
