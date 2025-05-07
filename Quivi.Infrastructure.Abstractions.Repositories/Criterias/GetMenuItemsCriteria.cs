namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetMenuItemsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public string? Search { get; init; }
        public IEnumerable<int>? ItemCategoryIds { get; init; }
        public DateTime? AvailableAtUtcDate { get; init; }
        public bool? IsDeleted { get; init; }
        public bool? Stock { get; init; }
        public bool? HasCategory { get; init; }

        public bool IncludeWeeklyAvailabilities { get; init; }
        public bool IncludeMenuItemCategoryAssociations { get; init; }
        public bool IncludeCategories { get; init; }
        public bool IncludeModifierGroups { get; init; }
        public bool IncludeTranslations { get; init; }
        public bool IncludeMenuItemModifiers { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
