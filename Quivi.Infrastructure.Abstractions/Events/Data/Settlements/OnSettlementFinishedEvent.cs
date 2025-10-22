namespace Quivi.Infrastructure.Abstractions.Events.Data.Settlements
{
    public record OnSettlementFinishedEvent : IEvent
    {
        public int Id { get; init; }
    }
}