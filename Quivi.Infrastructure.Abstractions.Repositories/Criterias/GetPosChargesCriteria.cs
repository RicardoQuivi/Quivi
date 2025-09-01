namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPosChargesCriteria : AGetPosChargesCriteria, IPagedCriteria
    {
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