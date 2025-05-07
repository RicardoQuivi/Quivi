namespace Quivi.Infrastructure.Abstractions.Events.Data.CustomChargeMethods
{
    public record OnCustomChargeMethodOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}