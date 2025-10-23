using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.SettlementServiceDetails
{
    public class GetSettlementServiceDetailsAsyncQuery : APagedAsyncQuery<SettlementServiceDetail>
    {
        public IEnumerable<int>? SettlementIds { get; init; }
        public IEnumerable<SettlementState>? SettlementStates { get; init; }
        public bool? IsMerchantDemo { get; init; }
    }

    internal class GetSettlementServiceDetailsAsyncQueryHandler : APagedQueryAsyncHandler<GetSettlementServiceDetailsAsyncQuery, SettlementServiceDetail>
    {
        private readonly ISettlementServiceDetailsRepository repository;

        public GetSettlementServiceDetailsAsyncQueryHandler(ISettlementServiceDetailsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<SettlementServiceDetail>> Handle(GetSettlementServiceDetailsAsyncQuery query)
        {
            return repository.GetAsync(new GetSettlementServiceDetailsCriteria
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