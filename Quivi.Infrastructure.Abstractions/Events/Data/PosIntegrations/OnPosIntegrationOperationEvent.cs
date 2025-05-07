namespace Quivi.Infrastructure.Abstractions.Events.Data.PosIntegrations
{
    public record OnPosIntegrationOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}
