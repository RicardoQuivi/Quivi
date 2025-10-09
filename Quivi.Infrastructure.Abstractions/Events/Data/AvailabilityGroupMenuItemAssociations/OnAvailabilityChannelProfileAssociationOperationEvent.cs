namespace Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroupMenuItemAssociations
{
    public record OnAvailabilityMenuItemAssociationOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int MerchantId { get; init; }
        public int MenuItemId { get; init; }
        public int AvailabilityGroupId { get; init; }
    }
}