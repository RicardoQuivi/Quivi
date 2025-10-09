namespace Quivi.Backoffice.Api.Requests.AvailabilityMenuItemAssociations
{
    public class UpdateAvailabilityMenuItemAssociationsRequest : ARequest
    {
        public required IEnumerable<UpdateAvailabilityMenuItemAssociation> Associations { get; init; }
    }

    public class UpdateAvailabilityMenuItemAssociation
    {
        public required string Id { get; init; }
        public bool Active { get; init; }
    }
}