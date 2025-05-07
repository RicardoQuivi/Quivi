namespace Quivi.Infrastructure.Abstractions.Events.Data.BackgroundJobs
{
    public record OnBackgroundJobOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public required string Id { get; init; }
        public int MerchantId { get; init; }
    }
}
