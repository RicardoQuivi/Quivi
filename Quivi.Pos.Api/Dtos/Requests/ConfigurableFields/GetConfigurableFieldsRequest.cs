namespace Quivi.Pos.Api.Dtos.Requests.ConfigurableFields
{
    public class GetConfigurableFieldsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
        public IEnumerable<string>? ChannelIds { get; init; }
        public bool? ForPosSessions { get; set; }
    }
}