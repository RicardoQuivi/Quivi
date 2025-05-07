namespace Quivi.Infrastructure.Abstractions.Events.Data.PreparationGroups
{
    public record OnPreparationGroupOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
        public bool IsCommited { get; init; }
    }
}