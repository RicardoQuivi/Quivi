using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Apis;

namespace Quivi.Backoffice.Api.Requests.MenuItems
{
    public class PatchMenuItemRequest
    {
        public string? Name { get; init; }
        public Optional<string> Description { get; init; }
        public Optional<string> ImageUrl { get; init; }
        public decimal? Price { get; init; }
        public PriceType? PriceType { get; init; }
        public decimal? VatRate { get; init; }
        public Optional<string> LocationId { get; init; }
        public IDictionary<Language, PatchMenuItemLanguage>? Translations { get; init; }
        public IEnumerable<string>? MenuCategoryIds { get; init; }
        public IEnumerable<string>? ModifierGroupIds { get; init; }
    }

    public class PatchMenuItemLanguage
    {
        public string? Name { get; init; }
        public Optional<string> Description { get; init; }
    }
}
