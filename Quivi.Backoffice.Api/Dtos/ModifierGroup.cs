using Quivi.Domain.Entities;

namespace Quivi.Backoffice.Api.Dtos
{
    public class ModifierGroup
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public int MinSelection { get; init; }
        public int MaxSelection { get; init; }
        public required IReadOnlyDictionary<string, ModifierGroupItem> Items { get; init; }
        public required IReadOnlyDictionary<Language, ModifierGroupTranslation> Translations { get; init; }
    }

    public class ModifierGroupItem
    {
        public decimal Price { get; init; }
        public int SortIndex { get; init; }
    }

    public class ModifierGroupTranslation
    {
        public required string Name { get; init; }
    }
}