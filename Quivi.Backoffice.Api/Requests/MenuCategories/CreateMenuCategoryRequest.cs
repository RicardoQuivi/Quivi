using Quivi.Domain.Entities;

namespace Quivi.Backoffice.Api.Requests.MenuCategories
{
    public class CreateMenuCategoryRequest : ARequest
    {
        public string Name { get; init; } = string.Empty;
        public string? ImageUrl { get; init; }
        public IDictionary<Language, CreateMenuCategoryLanguage>? Translations { get; init; }
    }

    public class CreateMenuCategoryLanguage
    {
        public required string Name { get; init; }
    }
}
