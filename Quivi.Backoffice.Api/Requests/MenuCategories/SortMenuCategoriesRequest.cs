namespace Quivi.Backoffice.Api.Requests.MenuCategories
{
    public class SortMenuCategoriesRequest : ARequest
    {
        public required IEnumerable<SortedMenuCategory> Items { get; init; }
    }

    public class SortedMenuCategory
    {
        public required string Id { get; init; }
        public int SortIndex { get; init; }
    }
}
