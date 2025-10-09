namespace Quivi.Backoffice.Api.Requests.AvailabilityMenuItemAssociations
{
    public class GetAvailabilityMenuItemAssociationsRequest : APagedRequest
    {
        public IEnumerable<string>? AvailabilityIds { get; init; }
        public IEnumerable<string>? MenuItemIds { get; init; }
    }
}