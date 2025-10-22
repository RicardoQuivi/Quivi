using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Settlements
{
    public class GetMerchantSettlementDetailsAsyncQuery : APagedAsyncQuery<MerchantSettlementDetail>
    {
        public IEnumerable<int>? SettlementIds { get; init; }
        public IEnumerable<ChargeMethod>? ChargeMethods { get; init; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
    }

    internal class GetMerchantSettlementDetailsAsyncQueryHandler : APagedQueryAsyncHandler<GetMerchantSettlementDetailsAsyncQuery, MerchantSettlementDetail>
    {
        private readonly ISettlementsRepository repository;

        public GetMerchantSettlementDetailsAsyncQueryHandler(ISettlementsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<MerchantSettlementDetail>> Handle(GetMerchantSettlementDetailsAsyncQuery query)
        {
            return repository.GetMerchantSettlementDetails(new GetMerchantSettlementDetailsCriteria
            {
                SettlementIds = query.SettlementIds,
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                ChargeMethods = query.ChargeMethods,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}