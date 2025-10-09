namespace Quivi.Backoffice.Api.Requests.AvailabilityChannelProfileAssociations
{
    public class GetAvailabilityChannelProfileAssociationsRequest : APagedRequest
    {
        public IEnumerable<string>? AvailabilityIds { get; init; }
        public IEnumerable<string>? ChannelProfileIds { get; init; }
    }
}