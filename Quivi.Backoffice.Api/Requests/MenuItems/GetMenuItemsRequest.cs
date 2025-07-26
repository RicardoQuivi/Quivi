namespace Quivi.Backoffice.Api.Requests.MenuItems
{
    public class GetMenuItemsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
        public string? ItemCategoryId { get; init; }
        public string? Search { get; init; }
        public bool? HasCategory { get; init; }
    }
}
