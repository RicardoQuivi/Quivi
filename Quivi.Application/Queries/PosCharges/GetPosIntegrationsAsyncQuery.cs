using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.PosCharges
{
    public class GetPosChargesAsyncQuery : APagedAsyncQuery<PosCharge>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<int>? OrderIds { get; init; }
        public bool? IsCaptured { get; init; }
        public bool IncludePosChargeSelectedMenuItems { get; init; }
        public bool IncludePosChargeInvoiceItems { get; init; }
        public bool IncludePosChargeSyncAttempts { get; init; }
        public bool IncludeMerchant { get; init; }
        public bool IncludeCharge { get; init; }
    }

    public class GetPosChargesAsyncQueryHandler : APagedQueryAsyncHandler<GetPosChargesAsyncQuery, PosCharge>
    {
        private readonly IPosChargesRepository repository;

        public GetPosChargesAsyncQueryHandler(IPosChargesRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<PosCharge>> Handle(GetPosChargesAsyncQuery query)
        {
            return repository.GetAsync(new GetPosChargesCriteria
            {
                MerchantIds = query.MerchantIds,
                Ids = query.Ids,
                SessionIds = query.SessionIds,
                OrderIds = query.OrderIds,
                IsCaptured = query.IsCaptured,

                IncludePosChargeSelectedMenuItems = query.IncludePosChargeSelectedMenuItems,
                IncludePosChargeInvoiceItems = query.IncludePosChargeInvoiceItems,
                IncludeMerchant = query.IncludeMerchant,
                IncludePosChargeSyncAttempts = query.IncludePosChargeSyncAttempts,
                IncludeCharge = query.IncludeCharge,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
