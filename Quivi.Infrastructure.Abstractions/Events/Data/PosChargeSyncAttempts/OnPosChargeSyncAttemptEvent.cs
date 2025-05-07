namespace Quivi.Infrastructure.Abstractions.Events.Data.PosChargeSyncAttempts
{
    public record OnPosChargeSyncAttemptEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int PosChargeId { get; init; }
        public int MerchantId { get; init; }
    }
}