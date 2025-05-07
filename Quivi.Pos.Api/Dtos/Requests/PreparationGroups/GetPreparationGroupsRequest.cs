namespace Quivi.Pos.Api.Dtos.Requests.PreparationGroups
{
    public class GetPreparationGroupsRequest : APagedRequest
    {
        public IEnumerable<string>? SessionIds { get; init; }
        public string? LocationId { get; init; }
        public bool? IsCommited { get; init; }
    }
}