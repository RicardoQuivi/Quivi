namespace Quivi.Backoffice.Api.Requests.Channels
{
    public class DeleteChannelsRequest
    {
        public required IEnumerable<string> Ids { get; init; }
    }
}
