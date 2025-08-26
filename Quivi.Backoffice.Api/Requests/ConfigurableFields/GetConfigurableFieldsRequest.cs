namespace Quivi.Backoffice.Api.Requests.ConfigurableFields
{
    public class GetConfigurableFieldsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
        public IEnumerable<string>? ChannelProfileIds { get; init; }
    }
}