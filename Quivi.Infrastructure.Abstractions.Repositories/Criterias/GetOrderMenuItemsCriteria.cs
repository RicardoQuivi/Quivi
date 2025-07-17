namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetOrderMenuItemsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? OrderIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }

        public bool IncludeMenuItem { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize {get; init; }
    }
}