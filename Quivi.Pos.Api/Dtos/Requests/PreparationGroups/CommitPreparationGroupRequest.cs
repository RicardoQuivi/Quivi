namespace Quivi.Pos.Api.Dtos.Requests.PreparationGroups
{
    public class CommitPreparationGroupRequest : ARequest
    {
        public bool IsPrepared { get; init; }
        public string? Note { get; init; }
        public IReadOnlyDictionary<string, int>? ItemsToCommit { get; init; }
        public string? LocationId { get; init; }
    }
}