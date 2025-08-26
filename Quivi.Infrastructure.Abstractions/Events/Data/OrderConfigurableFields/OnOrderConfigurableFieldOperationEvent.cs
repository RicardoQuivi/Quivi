namespace Quivi.Infrastructure.Abstractions.Events.Data.OrderConfigurableFields
{
    public record OnOrderConfigurableFieldOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}