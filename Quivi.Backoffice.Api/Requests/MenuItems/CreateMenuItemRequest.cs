using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;

namespace Quivi.Backoffice.Api.Requests.MenuItems
{
    public class CreateMenuItemRequest
    {
        public required string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? ImageUrl { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public PriceType PriceType { get; init; }
        public decimal VatRate { get; init; }
        public string? LocationId { get; init; }
        public IDictionary<Language, CreateMenuItemLanguage>? Translations { get; init; }
    }

    public class CreateMenuItemLanguage
    {
        public required string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
    }
}
