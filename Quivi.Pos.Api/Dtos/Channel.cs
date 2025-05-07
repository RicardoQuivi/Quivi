namespace Quivi.Pos.Api.Dtos
{
    public class Channel
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Url { get; init; }
        public required string ChannelProfileId { get; init; }
    }
}
