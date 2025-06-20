namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPrinterWorkersCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<string>? Identifiers { get; init; }
        public bool? IsDeleted { get; init; }
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}