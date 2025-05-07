namespace Quivi.Infrastructure.Abstractions.Events.Data.Locations
{
    public record OnLocationOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}