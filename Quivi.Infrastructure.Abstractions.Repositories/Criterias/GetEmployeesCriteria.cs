namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetEmployeesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<string>? Names { get; init; }
        public bool? IsDeleted { get; init; }

        public bool IncludeMerchant { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
