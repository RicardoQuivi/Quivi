namespace Quivi.Backoffice.Api.Responses.Channels
{
    public class DeleteChannelsRequest
    {
        public required IEnumerable<string> Ids { get; init; }
    }
}
