namespace Quivi.Pos.Api.Dtos.Requests.PosIntegrations
{
    public class GetPosIntegrationsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; set; }
    }
}
