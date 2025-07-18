using Quivi.Backoffice.Api.Dtos;

namespace Quivi.Backoffice.Api.Requests.Transactions
{
    public abstract class AGetTransactionsRequest : ARequest
    {
        public string? Search { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public SynchronizationState? SyncState { get; init; }
        public bool? OverPayed { get; init; }
        public bool? HasDiscounts { get; init; }
        public bool? HasReviewComment { get; init; }
        public bool? AdminView { get; init; }
        public string? CustomChargeMethodId { get; init; }
        public bool? QuiviPaymentsOnly { get; init; }
        public bool? HasRefunds { get; init; }
    }
}