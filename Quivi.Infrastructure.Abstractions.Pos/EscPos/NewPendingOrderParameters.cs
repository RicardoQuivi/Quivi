namespace Quivi.Infrastructure.Abstractions.Pos.EscPos
{
    public class NewPendingOrderParameters
    {
        public required string Title { get; init; }
        public required string OrderPlaceholder { get; init; }
        public DateTime Timestamp { get; init; }
        public required string ChannelPlaceholder { get; init; }
    }
}