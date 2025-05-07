using Quivi.Domain.Entities;

namespace Quivi.Backoffice.Api.Requests.ModifierGroups
{
    public class PatchModifierGroupRequest : ARequest
    {
        public string? Name { get; init; }
        public int? MinSelection { get; init; }
        public int? MaxSelection { get; init; }
        public IDictionary<string, PatchModifierItem>? Items { get; init; }
        public IDictionary<Language, PatchModifierGroupLanguage>? Translations { get; init; }
    }

    public class PatchModifierItem
    {
        public decimal? Price { get; init; }
        public int? SortIndex { get; init; }
    }

    public class PatchModifierGroupLanguage
    {
        public string? Name { get; init; }
    }
}