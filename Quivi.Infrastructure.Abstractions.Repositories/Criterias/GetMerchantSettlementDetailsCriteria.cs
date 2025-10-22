using Quivi.Domain.Entities.Charges;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetMerchantSettlementDetailsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? SettlementIds { get; init; }
        public IEnumerable<ChargeMethod>? ChargeMethods { get; init; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}