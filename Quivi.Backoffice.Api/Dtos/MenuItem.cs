using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;

namespace Quivi.Backoffice.Api.Dtos
{
    public class MenuItem
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public bool Stock { get; init; }
        public decimal Price { get; init; }
        public PriceType PriceType { get; init; }
        public bool ShowWhenNotAvailable { get; init; }
        public decimal VatRate { get; init; }
        public int SortIndex { get; init; }
        public string? LocationId { get; init; }
        public required IEnumerable<string> MenuCategoryIds { get; init; }
        public required IEnumerable<string> ModifierGroupIds { get; init; }
        public required IReadOnlyDictionary<Language, MenuItemTranslation> Translations { get; init; }
    }

    public class MenuItemTranslation
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
    }
}
