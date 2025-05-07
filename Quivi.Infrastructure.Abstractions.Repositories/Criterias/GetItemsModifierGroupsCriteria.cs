namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetItemsModifierGroupsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? MenuItemIds { get; init; }
        public bool? IsDeleted { get; init; }

        public bool IncludeTranslations { get; init; }
        public bool IncludeModifiers { get; init; }
        public bool IncludeParentMenuItems { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}