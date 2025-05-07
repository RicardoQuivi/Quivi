using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.PosIntegrations
{
    public class GetPosIntegrationsAsyncQuery : APagedAsyncQuery<PosIntegration>
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
    }

    public class GetPosIntegrationsAsyncQueryHandler : APagedQueryAsyncHandler<GetPosIntegrationsAsyncQuery, PosIntegration>
    {
        private readonly IPosIntegrationsRepository repository;

        public GetPosIntegrationsAsyncQueryHandler(IPosIntegrationsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<PosIntegration>> Handle(GetPosIntegrationsAsyncQuery query)
        {
            return repository.GetAsync(new GetPosIntegrationsCriteria
            {
                Ids = query.Ids,
                ChannelIds = query.ChannelIds,
                OrderIds = query.OrderIds,
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                ChargeIds = query.ChargeIds,
                SessionIds = query.SessionIds,
                IsDeleted = query.IsDeleted,
                Types = query.Types,
                SyncStates = query.SyncStates,
                IsDiagnosticErrorsMuted = query.IsDiagnosticErrorsMuted,
                HasQrCodes = query.HasQrCodes,
                    
                IncludeChannels = query.IncludeChannels,
                IncludeMerchant = query.IncludeMerchant,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
