using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.PosChargeSyncAttemptSyncAttempts
{
    public class GetPosChargeSyncAttemptsAsyncQuery : APagedAsyncQuery<PosChargeSyncAttempt>
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? PosChargeIds { get; init; }
        public IEnumerable<SyncAttemptState>? States { get; init; }
        public IEnumerable<SyncAttemptType>? Types { get; init; }
    }

    public class GetPosChargeSyncAttemptsAsyncQueryHandler : APagedQueryAsyncHandler<GetPosChargeSyncAttemptsAsyncQuery, PosChargeSyncAttempt>
    {
        private readonly IPosChargeSyncAttemptsRepository repository;

        public GetPosChargeSyncAttemptsAsyncQueryHandler(IPosChargeSyncAttemptsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<PosChargeSyncAttempt>> Handle(GetPosChargeSyncAttemptsAsyncQuery query)
        {
            return repository.GetAsync(new GetPosChargeSyncAttemptsCriteria
            {
                Ids = query.Ids,
                PosChargeIds = query.PosChargeIds,
                States = query.States,
                Types = query.Types,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}