using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPosChargeSyncAttemptsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? PosChargeIds { get; init; }
        public IEnumerable<SyncAttemptState>? States { get; init; }
        public IEnumerable<SyncAttemptType>? Types { get; init; }
        public bool IncludePosCharge { get; init; }
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}