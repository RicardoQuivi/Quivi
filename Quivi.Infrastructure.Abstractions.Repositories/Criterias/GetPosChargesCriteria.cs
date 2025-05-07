namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPosChargesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; set; }
        public bool IncludePosChargeSelectedMenuItems { get; init; }
        public bool IncludePosChargeInvoiceItems { get; init; }
        public bool IncludeMerchant { get; init; }
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}