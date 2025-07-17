namespace Quivi.Infrastructure.Abstractions.Events.Data.Reviews
{
    public record OnReviewOperationEvent : IOperationEvent
    {
        public required EntityOperation Operation { get; init; }
        public required int Id { get; init; }
    }
}
