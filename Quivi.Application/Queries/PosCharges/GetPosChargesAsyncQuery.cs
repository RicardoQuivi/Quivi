using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.PosCharges
{
    public class GetPosChargesAsyncQuery : AGetPosChargesQuery, IQuery<Task<IPagedData<PosCharge>>>
    {
        public bool IncludePosChargeSelectedMenuItems { get; init; }
        public bool IncludePosChargeInvoiceItems { get; init; }
        public bool IncludePosChargeInvoiceItemsOrderMenuItems { get; init; }
        public bool IncludePosChargeSyncAttempts { get; init; }
        public bool IncludeMerchantCustomCharge { get; init; }
        public bool IncludeMerchantCustomChargeCustomChargeMethod { get; init; }
        public bool IncludeMerchant { get; init; }
        public bool IncludeCharge { get; init; }
        public bool IncludeAcquirerCharge { get; init; }

        public int PageIndex { get; init; } = 0;
        public int? PageSize { get; init; } = 0;
        public SortDirection SortDirection { get; init; }
    }

    internal class GetPosChargesAsyncQueryHandler : APagedQueryAsyncHandler<GetPosChargesAsyncQuery, PosCharge>
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
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                Ids = query.Ids,
                SessionIds = query.SessionIds,
                OrderIds = query.OrderIds,
                IsCaptured = query.IsCaptured,
                FromCapturedDate = query.FromCapturedDate,
                ToCapturedDate = query.ToCapturedDate,
                HasSession = query.HasSession,
                HasDiscounts = query.HasDiscounts,
                HasReview = query.HasReview,
                HasReviewComment = query.HasReviewComment,
                CustomChargeMethodIds = query.CustomChargeMethodIds,
                HasRefunds = query.HasRefunds,
                SyncingState = query.SyncingState,
                QuiviPaymentsOnly = query.QuiviPaymentsOnly,

                IncludePosChargeSelectedMenuItems = query.IncludePosChargeSelectedMenuItems,
                IncludePosChargeInvoiceItems = query.IncludePosChargeInvoiceItems,
                IncludePosChargeInvoiceItemsOrderMenuItems = query.IncludePosChargeInvoiceItemsOrderMenuItems,
                IncludeMerchant = query.IncludeMerchant,
                IncludePosChargeSyncAttempts = query.IncludePosChargeSyncAttempts,
                IncludeMerchantCustomCharge = query.IncludeMerchantCustomCharge,
                IncludeMerchantCustomChargeCustomChargeMethod = query.IncludeMerchantCustomChargeCustomChargeMethod,
                IncludeCharge = query.IncludeCharge,
                IncludeAcquirerCharge = query.IncludeAcquirerCharge,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,

                SortDirection = query.SortDirection,
            });
        }
    }
}
