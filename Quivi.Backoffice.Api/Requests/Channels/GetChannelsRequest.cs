namespace Quivi.Backoffice.Api.Requests.Channels
{
    public class GetChannelsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; set; }
        public string? Search { get; set; }
        public bool AllowsSessionsOnly { get; set; }
    }
}
