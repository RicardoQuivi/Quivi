using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public abstract record AGetPosChargesCriteria
    {
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? OrderIds { get; init; }
        public IEnumerable<int>? CustomChargeMethodIds { get; init; }
        public IEnumerable<int>? LocationIds { get; set; }

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
    }
}