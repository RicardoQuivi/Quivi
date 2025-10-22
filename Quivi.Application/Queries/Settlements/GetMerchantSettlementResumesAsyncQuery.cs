using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Settlements
{
    public class GetMerchantSettlementResumesAsyncQuery : APagedAsyncQuery<MerchantSettlementResume>
    {
        public IEnumerable<int>? SettlementIds { get; init; }
        public IEnumerable<DateOnly>? Dates { get; init; }
        public IEnumerable<ChargeMethod>? ChargeMethods { get; init; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
    }

    internal class GetMerchantSettlementResumesAsyncQueryHandler : APagedQueryAsyncHandler<GetMerchantSettlementResumesAsyncQuery, MerchantSettlementResume>
    {
        private readonly ISettlementsRepository settlementsRepository;

        public GetMerchantSettlementResumesAsyncQueryHandler(ISettlementsRepository settlementsRepository)
        {
            this.settlementsRepository = settlementsRepository;
        }

        public override Task<IPagedData<MerchantSettlementResume>> Handle(GetMerchantSettlementResumesAsyncQuery query)
        {
            return settlementsRepository.GetMerchantSettlementResumes(new GetMerchantSettlementResumesCriteria
            {
                SettlementIds = query.SettlementIds,
                Dates = query.Dates,
                ChargeMethods = query.ChargeMethods,
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}