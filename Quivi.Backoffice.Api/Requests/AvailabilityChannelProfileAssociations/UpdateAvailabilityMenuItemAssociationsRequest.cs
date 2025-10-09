namespace Quivi.Backoffice.Api.Requests.AvailabilityChannelProfileAssociations
{
    public class UpdateAvailabilityChannelProfileAssociationsRequest : ARequest
    {
        public required IEnumerable<UpdateAvailabilityChannelProfileAssociation> Associations { get; init; }
    }

    public class UpdateAvailabilityChannelProfileAssociation
    {
        public required string Id { get; init; }
        public bool Active { get; init; }
    }
}