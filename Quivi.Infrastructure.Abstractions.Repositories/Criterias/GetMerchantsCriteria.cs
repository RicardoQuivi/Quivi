namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public class GetMerchantsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? ApplicationUserIds { get; set; }
        public IEnumerable<int>? ParentIds { get; set; }
        public IEnumerable<int>? ChildIds { get; set; }
        public IEnumerable<int>? Ids { get; set; }
        public IEnumerable<string>? VatNumbers { get; set; }
        public string? Search { get; init; }
        public bool? Inactive { get; init; }
        public bool? IsParentMerchant { get; init; }
        public bool IncludeChildMerchants { get; init; }
        public bool IncludeParentMerchant { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
