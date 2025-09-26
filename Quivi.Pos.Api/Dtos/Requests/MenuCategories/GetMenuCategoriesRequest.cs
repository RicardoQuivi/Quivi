namespace Quivi.Pos.Api.Dtos.Requests.MenuCategories
{
    public class GetMenuCategoriesRequest : APagedRequest
    {
        public string? Search { get; init; }
        public IEnumerable<string>? Ids { get; init; }
        public bool? HasItems { get; init; }
    }
}