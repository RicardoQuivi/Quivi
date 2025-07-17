namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPosChargesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; set; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? OrderIds { get; init; }
        public bool? IsCaptured { get; init; }
        public bool? HasSession { get; init; }

        public bool IncludePosChargeSelectedMenuItems { get; init; }
        public bool IncludePosChargeInvoiceItems { get; init; }
        public bool IncludePosChargeSyncAttempts { get; init; }
        public bool IncludeMerchant { get; init; }
        public bool IncludeCharge { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}