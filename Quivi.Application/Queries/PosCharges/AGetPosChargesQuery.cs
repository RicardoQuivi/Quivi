using Quivi.Domain.Entities.Pos;

namespace Quivi.Application.Queries.PosCharges
{
    public abstract class AGetPosChargesQuery
    {
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<int>? OrderIds { get; init; }
        public IEnumerable<int>? CustomChargeMethodIds { get; set; }
        public IEnumerable<int>? LocationIds { get; set; }
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
    }
}