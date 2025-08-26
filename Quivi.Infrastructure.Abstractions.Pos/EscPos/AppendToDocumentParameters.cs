namespace Quivi.Infrastructure.Abstractions.Pos.EscPos
{
    public class AppendToDocumentParameters
    {
        public required byte[] EscPosContent { get; init; }
        public required string ChannelName { get; init; }
        public IEnumerable<string>? AdditionalInfo { get; init; }
    }
}