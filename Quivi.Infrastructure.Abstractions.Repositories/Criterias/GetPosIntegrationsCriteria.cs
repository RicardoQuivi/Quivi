using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public class GetPosIntegrationsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? OrderIds { get; init; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? ChargeIds { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<IntegrationType>? Types { get; init; }
        public bool? HasQrCodes { get; init; }
        public IEnumerable<SyncState>? SyncStates { get; init; }
        public bool? IsDiagnosticErrorsMuted { get; init; }
        public bool? IsDeleted { get; init; } = false;

        public bool IncludeChannels { get; init; }
        public bool IncludeMerchant { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
