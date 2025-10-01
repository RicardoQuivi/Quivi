namespace Quivi.Infrastructure.Abstractions.Events.Data.Users
{
    public record OnUserEmailTokenGeneratedEvent : IEvent
    {
        public int Id { get; init; }
        public required string Code { get; init; }
        public UserAppType? Type { get; init; }
    }
}
