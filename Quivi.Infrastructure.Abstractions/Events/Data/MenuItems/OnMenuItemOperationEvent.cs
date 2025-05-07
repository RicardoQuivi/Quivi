namespace Quivi.Infrastructure.Abstractions.Events.Data.MenuItems
{
    public record OnMenuItemOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}