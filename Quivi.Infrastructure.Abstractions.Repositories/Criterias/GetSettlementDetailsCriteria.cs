using Quivi.Domain.Entities.Financing;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetSettlementDetailsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? SettlementIds { get; init; }
        public IEnumerable<SettlementState>? SettlementStates { get; init; }
        public bool? IsMerchantDemo { get; init; }
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}