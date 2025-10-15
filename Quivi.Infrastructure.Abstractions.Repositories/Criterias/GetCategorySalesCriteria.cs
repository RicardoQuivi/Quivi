namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetCategorySalesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }
        public SalesPeriod? Period { get; init; }
        public ProductSalesSortBy SortBy { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}