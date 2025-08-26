namespace Quivi.Infrastructure.Abstractions.Events.Data.OrderConfigurableFieldChannelProfileAssociations
{
    public record OnOrderConfigurableFieldChannelProfileAssociationOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int MerchantId { get; init; }
        public int ChannelProfileId { get; init; }
        public int OrderConfigurableFieldId { get; init; }
    }
}