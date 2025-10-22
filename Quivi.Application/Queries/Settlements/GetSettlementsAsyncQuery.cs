using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Settlements
{
    public class GetSettlementsAsyncQuery : APagedAsyncQuery<Settlement>
    {
        public IEnumerable<DateOnly>? Dates { get; init; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<SettlementState>? States { get; init; }

        public bool IncludeSettlementDetails { get; init; }
        public bool IncludeSettlementServiceDetails { get; init; }
    }

    internal class GetSettlementsAsyncQueryHandler : APagedQueryAsyncHandler<GetSettlementsAsyncQuery, Settlement>
    {
        private readonly ISettlementsRepository settlementsRepository;

        public GetSettlementsAsyncQueryHandler(ISettlementsRepository settlementsRepository)
        {
            this.settlementsRepository = settlementsRepository;
        }

        public override Task<IPagedData<Settlement>> Handle(GetSettlementsAsyncQuery query)
        {
            return settlementsRepository.GetAsync(new GetSettlementsCriteria
            {
                Dates = query.Dates,
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                States = query.States,

                IncludeSettlementDetails = query.IncludeSettlementDetails,
                IncludeSettlementServiceDetails = query.IncludeSettlementServiceDetails,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}