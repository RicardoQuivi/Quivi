namespace Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroups
{
    public record OnAvailabilityGroupOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}