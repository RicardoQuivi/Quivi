namespace Quivi.Backoffice.Api.Requests.MenuCategories
{
    public class GetMenuCategoriesRequest : ARequest
    {
        public IEnumerable<string>? Ids { get; init; }
    }
}
