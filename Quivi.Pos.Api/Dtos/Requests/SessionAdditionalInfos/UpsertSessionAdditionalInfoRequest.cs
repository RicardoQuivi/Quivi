namespace Quivi.Pos.Api.Dtos.Requests.SessionAdditionalInfos
{
    public class UpsertSessionAdditionalInfoRequest : ARequest
    {
        public required IReadOnlyDictionary<string, string> Fields { get; init; }
    }
}