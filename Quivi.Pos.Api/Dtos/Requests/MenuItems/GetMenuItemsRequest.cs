namespace Quivi.Pos.Api.Dtos.Requests.MenuItems
{
    public class GetMenuItemsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
        public string? MenuCategoryId { get; init; }
        public string? Search { get; init; }
        public bool IncludeDeleted { get; init; } = false;
    }
}
