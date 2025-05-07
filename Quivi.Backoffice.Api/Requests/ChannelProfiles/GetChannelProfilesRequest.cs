namespace Quivi.Backoffice.Api.Requests.ChannelProfiles
{
    public class GetChannelProfilesRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; set; }
        public IEnumerable<string>? ChannelIds { get; set; }
    }
}
