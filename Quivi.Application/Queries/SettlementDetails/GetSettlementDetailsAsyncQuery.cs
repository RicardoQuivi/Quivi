using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.SettlementDetails
{
    public class GetSettlementDetailsAsyncQuery : APagedAsyncQuery<SettlementDetail>
    {
        public IEnumerable<int>? SettlementIds { get; init; }
        public IEnumerable<SettlementState>? SettlementStates { get; init; }
        public bool? IsMerchantDemo { get; init; }
    }

    internal class GetSettlementDetailsAsyncQueryHandler : APagedQueryAsyncHandler<GetSettlementDetailsAsyncQuery, SettlementDetail>
    {
        private readonly ISettlementDetailsRepository repository;

        public GetSettlementDetailsAsyncQueryHandler(ISettlementDetailsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<SettlementDetail>> Handle(GetSettlementDetailsAsyncQuery query)
        {
            return repository.GetAsync(new GetSettlementDetailsCriteria
            {
                SettlementIds = query.SettlementIds,
                SettlementStates = query.SettlementStates,
                IsMerchantDemo = query.IsMerchantDemo,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}