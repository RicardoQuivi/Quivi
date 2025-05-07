namespace Quivi.Pos.Api.Dtos.Requests.MenuCategories
{
    public class GetMenuCategoriesRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
        public bool? HasItems { get; init; }
    }
}
