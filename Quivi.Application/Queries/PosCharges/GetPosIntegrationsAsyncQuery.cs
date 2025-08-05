using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.PosCharges
{
    public class GetPosChargesAsyncQuery : APagedAsyncQuery<PosCharge>
    {
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<int>? OrderIds { get; init; }
        public IEnumerable<int>? CustomChargeMethodIds { get; set; }
        public bool? IsCaptured { get; init; }
        public bool? HasSession { get; init; }
        public DateTime? FromCapturedDate { get; set; }
        public DateTime? ToCapturedDate { get; set; }
        public bool? HasDiscounts { get; set; }
        public bool? HasReview { get; set; }
        public bool? HasReviewComment { get; set; }
        public bool? HasRefunds { get; init; }
        public bool? QuiviPaymentsOnly { get; init; }
        public SyncAttemptState? SyncingState { get; init; }

        public bool IncludePosChargeSelectedMenuItems { get; init; }
        public bool IncludePosChargeInvoiceItems { get; init; }
        public bool IncludePosChargeInvoiceItemsOrderMenuItems { get; init; }
        public bool IncludePosChargeSyncAttempts { get; init; }
        public bool IncludeMerchantCustomCharge { get; init; }
        public bool IncludeMerchantCustomChargeCustomChargeMethod { get; init; }
        public bool IncludeMerchant { get; init; }
        public bool IncludeCharge { get; init; }
        public bool IncludeAcquirerCharge { get; init; }
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
            });
        }
    }
}
