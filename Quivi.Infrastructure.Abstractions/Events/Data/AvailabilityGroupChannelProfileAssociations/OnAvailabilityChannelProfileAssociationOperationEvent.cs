namespace Quivi.Infrastructure.Abstractions.Events.Data.AvailabilityGroupChannelProfileAssociations
{
    public record OnAvailabilityChannelProfileAssociationOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int MerchantId { get; init; }
        public int ChannelProfileId { get; init; }
        public int AvailabilityGroupId { get; init; }
    }
}