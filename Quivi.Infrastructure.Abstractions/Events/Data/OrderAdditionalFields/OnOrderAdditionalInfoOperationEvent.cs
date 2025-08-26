namespace Quivi.Infrastructure.Abstractions.Events.Data.OrderAdditionalFields
{
    public record OnOrderAdditionalInfoOperationEvent : IEvent
    {
        public EntityOperation Operation { get; init; }
        public int OrderConfigurableFieldId { get; init; }
        public int OrderId { get; init; }
        public int MerchantId { get; init; }
    }
}