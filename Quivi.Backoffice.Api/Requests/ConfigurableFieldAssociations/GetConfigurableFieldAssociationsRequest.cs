namespace Quivi.Backoffice.Api.Requests.ConfigurableFieldAssociations
{
    public class GetConfigurableFieldAssociationsRequest : APagedRequest
    {
        public IEnumerable<string>? ChannelProfileIds { get; init; }
        public IEnumerable<string>? ConfigurableFieldIds { get; init; }
    }
}