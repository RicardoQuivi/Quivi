namespace Quivi.Backoffice.Api.Requests.Channels
{
    public class CreateChannelsRequest
    {
        public required IEnumerable<AddChannel> Data { get; init; }
        public required string ChannelProfileId { get; init; } = string.Empty;
    }

    public class AddChannel
    {
        public required string Name { get; init; } = string.Empty;
    }
}