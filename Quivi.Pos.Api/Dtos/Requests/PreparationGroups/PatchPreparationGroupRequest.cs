namespace Quivi.Pos.Api.Dtos.Requests.PreparationGroups
{
    public class PatchPreparationGroupRequest
    {
        public required IReadOnlyDictionary<string, int> Items { get; init; }
    }
}