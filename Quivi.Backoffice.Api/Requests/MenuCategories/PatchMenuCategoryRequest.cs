using Quivi.Domain.Entities;
using Quivi.Infrastructure.Apis;

namespace Quivi.Backoffice.Api.Requests.MenuCategories
{
    public class PatchMenuCategoryRequest : ARequest
    {
        public string? Name { get; init; }
        public Optional<string> ImageUrl { get; init; }
        public IDictionary<Language, PatchMenuCategoryLanguage?>? Translations { get; init; }
    }

    public class PatchMenuCategoryLanguage
    {
        public string? Name { get; init; }
    }
}
