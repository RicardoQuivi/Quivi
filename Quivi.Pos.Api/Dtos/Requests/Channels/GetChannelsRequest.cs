namespace Quivi.Pos.Api.Dtos.Requests.Channels
{
    public class GetChannelsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; set; }
        public IEnumerable<string>? SessionIds { get; set; }
        public string? ChannelProfileId { get; set; }
        public string? Search { get; set; }
        public bool AllowsSessionsOnly { get; set; }
        public bool AllowsPrePaidOrderingOnly { get; set; }
        public bool AllowsPostPaidOrderingOnly { get; set; }
        public bool IncludeDeleted { get; set; }
        public bool IncludePageRanges { get; set; }
        public bool? HasOpenSession { get; set; }
    }
}