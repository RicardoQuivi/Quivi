namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetOrderSequencesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
