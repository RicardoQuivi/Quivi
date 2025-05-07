namespace Quivi.Backoffice.Api.Dtos
{
    public class Channel
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public bool IsActive { get; init; }
        public required string Url { get; init; }
        public required string ChannelProfileId { get; init; }
    }
}
