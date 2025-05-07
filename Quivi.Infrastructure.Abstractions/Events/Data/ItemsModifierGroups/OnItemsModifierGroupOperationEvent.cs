namespace Quivi.Infrastructure.Abstractions.Events.Data.ItemsModifierGroups
{
    public record OnItemsModifierGroupOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}