namespace Quivi.Pos.Api.Dtos
{
    public class Session
    {
        public required string Id { get; init; }
        public string? EmployeeId { get; init; }
        public required string ChannelId { get; init; }
        public required bool IsOpen { get; init; }
        public DateTimeOffset? ClosedAt { get; init; }
        public bool IsDeleted { get; init; }
        public required IEnumerable<SessionItem> Items { get; init; }
    }
}