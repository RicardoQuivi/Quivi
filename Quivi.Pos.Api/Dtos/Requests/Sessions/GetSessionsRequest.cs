namespace Quivi.Pos.Api.Dtos.Requests.Sessions
{
    public class GetSessionsRequest : APagedRequest
    {
        public IEnumerable<string>? ChannelIds { get; set; }
        public IEnumerable<string>? Ids { get; set; }
        public bool? IsOpen { get; set; }
        public bool IncludeDeleted { get; set; }
    }
}
