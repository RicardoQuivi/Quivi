namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPosChargeInvoiceItemCriteria : IPagedCriteria
    {
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? PosChargeIds { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public bool? IsParent { get; init; }

        public bool IncludeOrderMenuItem { get; init; }
        public bool IncludePosChargeChargeInvoiceDocuments { get; init; }
        public bool IncludePosChargeChargeMerchantCustomChargeCustomChargeMethod { get; init; }
        public bool IncludeChildrenPosChargeInvoiceItems { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}