namespace Quivi.Backoffice.Api.Requests.Channels
{
    public class PatchChannelsRequest
    {
        public required IEnumerable<string> Ids { get; set; }
        public required string Name { get; init; }
        public string? ChannelProfileId { get; init; }
    }
}
