namespace Quivi.Infrastructure.Abstractions.Pos.EscPos
{
    public class TestPrinterParameters
    {
        public required string Title { get; init; }
        public required string Message { get; init; }
        public bool PingOnly { get; init; }
        public DateTime Timestamp { get; init; }
    }
}
