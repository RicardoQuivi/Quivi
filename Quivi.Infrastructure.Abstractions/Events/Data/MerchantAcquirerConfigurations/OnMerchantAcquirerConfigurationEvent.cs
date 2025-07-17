namespace Quivi.Infrastructure.Abstractions.Events.Data.MerchantAcquirerConfigurations
{
    public record OnMerchantAcquirerConfigurationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int MerchantId { get; init; }
        public int Id { get; init; }
    }
}