using Quivi.Domain.Entities.Financing;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetSettlementsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<DateOnly>? Dates { get; init; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<SettlementState>? States { get; init; }

        public bool IncludeSettlementDetails { get; init; }
        public bool IncludeSettlementServiceDetails { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}