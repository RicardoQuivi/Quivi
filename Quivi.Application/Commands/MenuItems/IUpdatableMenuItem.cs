using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;

namespace Quivi.Application.Commands.MenuItems
{
    public interface IMenuItemTranslation
    {
        Language Language { get; }
        string Name { get; set; }
        string? Description { get; set; }
    }

    public interface IUpdatableItemCategory : IUpdatableEntity
    {
        public int Id { get; }
    }

    public interface IUpdatableModifierGroup : IUpdatableEntity
    {
        public int Id { get; }
    }

    public interface IUpdatableAvailabilityGroup : IUpdatableEntity
    {
        public int Id { get; }
    }

    public interface IUpdatableMenuItem : IUpdatableEntity
    {
        int MerchantId { get; }
        int Id { get; }
        string Name { get; set; }
        string? Description { get; set; }
        decimal Price { get; set; }
        PriceType PriceType { get; set; }
        decimal VatRate { get; set; }
        int? LocationId { get; set; }
        string? ImageUrl { get; set; }
        int SortIndex { get; set; }
        bool HasStock { get; set; }

        IUpdatableTranslations<IMenuItemTranslation> Translations { get; }
        IUpdatableRelationship<IUpdatableItemCategory, int> Categories { get; }
        IUpdatableRelationship<IUpdatableModifierGroup, int> ModifierGroups { get; }
        IUpdatableRelationship<IUpdatableAvailabilityGroup, int> AvailabilityGroups { get; }
    }
}