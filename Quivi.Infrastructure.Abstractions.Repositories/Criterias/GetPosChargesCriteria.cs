using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPosChargesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? OrderIds { get; init; }
        public IEnumerable<int>? CustomChargeMethodIds { get; init; }

        public bool? IsCaptured { get; init; }
        public bool? HasSession { get; init; }
        public DateTime? FromCapturedDate { get; init; }
        public DateTime? ToCapturedDate { get; init; }
        public bool? HasDiscounts { get; init; }
        public bool? HasReview { get; init; }
        public bool? HasReviewComment { get; init; }
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
        public bool IncludeReview { get; init; }
        public bool IncludeAcquirerCharge { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}