namespace Quivi.Guests.Api.Dtos.Requests.MenuItems
{
    public class GetMenuItemsRequest : APagedRequest
    {
        public required string ChannelId { get; init; }
        public string? MenuItemCategoryId { get; init; }
        public IEnumerable<string>? Ids { get; init; }
        public DateTimeOffset? AtDate { get; init; }
        public bool IgnoreCalendarAvailability { get; init; }
    }
}