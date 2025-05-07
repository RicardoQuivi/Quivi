namespace Quivi.Pos.Api.Dtos.Requests.ChannelProfiles
{
    public class GetChannelProfilesRequest : APagedRequest
    {
        public bool AllowsSessionsOnly { get; set; }
        public bool? HasChannels { get; set; }
    }
}