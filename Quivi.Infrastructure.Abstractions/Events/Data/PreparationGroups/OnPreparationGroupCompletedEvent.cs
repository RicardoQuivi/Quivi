namespace Quivi.Infrastructure.Abstractions.Events.Data.PreparationGroups
{
    public record OnPreparationGroupCompletedEvent : IEvent
    {
        public int Id { get; init; }
        public int MerchantId { get; init; }
        public int? ParentPreparationGroupId { get; init; }
    }
}