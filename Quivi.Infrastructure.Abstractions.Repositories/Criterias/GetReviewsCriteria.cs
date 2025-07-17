namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetReviewsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? PosChargeIds { get; init; }
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}