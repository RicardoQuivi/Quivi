using Quivi.Domain.Entities;

namespace Quivi.Backoffice.Api.Requests.ModifierGroups
{
    public class CreateModifierGroupRequest : ARequest
    {
        public required string Name { get; init; } = string.Empty;
        public int MinSelection { get; init; }
        public int MaxSelection { get; init; }
        public required IDictionary<string, CreateModifierItem> Items { get; init; } = new Dictionary<string, CreateModifierItem>();
        public IDictionary<Language, CreateModifierGroupLanguage>? Translations { get; init; }
    }

    public class CreateModifierItem
    {
        public decimal Price { get; init; }
        public int SortIndex { get; init; }
    }

    public class CreateModifierGroupLanguage
    {
        public required string Name { get; init; }
    }
}