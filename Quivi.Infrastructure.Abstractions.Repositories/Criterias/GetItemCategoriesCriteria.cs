namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetItemCategoriesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MenuItemIds { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public string? Name { get; init; }
        public bool? IsDeleted { get; init; }
        public bool? WithItems { get; init; }
        public DateTime? AvailableAtUtcDate { get; init; }

        public bool IncludeMenuItems { get; init; }
        public bool IncludeTranslations { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
