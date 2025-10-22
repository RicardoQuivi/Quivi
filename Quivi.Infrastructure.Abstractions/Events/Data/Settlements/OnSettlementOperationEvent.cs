namespace Quivi.Infrastructure.Abstractions.Events.Data.Settlements
{
    public record OnSettlementOperationEvent : IOperationEvent
    {
        public int Id { get; init; }
        public EntityOperation Operation { get; init; }
    }
}