using Quivi.Domain.Entities;

namespace Quivi.Backoffice.Api.Dtos
{
    public class MenuCategory
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public string? ImageUrl { get; init; }
        public int SortIndex { get; init; }
        public required IReadOnlyDictionary<Language, MenuCategoryTranslation> Translations { get; init; }
    }

    public class MenuCategoryTranslation
    {
        public required string Name { get; init; }
    }
}
