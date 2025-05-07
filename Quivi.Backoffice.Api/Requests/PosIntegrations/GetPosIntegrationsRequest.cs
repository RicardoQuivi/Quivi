namespace Quivi.Backoffice.Api.Requests.PosIntegrations
{
    public class GetPosIntegrationsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
        public string? ChannelId { get; init; }
    }
}
