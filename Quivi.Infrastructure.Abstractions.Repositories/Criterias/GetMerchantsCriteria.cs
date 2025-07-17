namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetMerchantsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? ApplicationUserIds { get; init; }
        public IEnumerable<int>? ParentIds { get; init; }
        public IEnumerable<int>? ChildIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<string>? VatNumbers { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public string? Search { get; init; }
        public bool? IsDeleted { get; init; }
        public bool? IsParentMerchant { get; init; }
        public bool IncludeChildMerchants { get; init; }
        public bool IncludeParentMerchant { get; init; }
        public bool IncludeFees { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
