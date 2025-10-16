using Quivi.Domain.Entities.Charges;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPartnerChargeMethodSalesCriteria : IPagedCriteria
    {
        public IEnumerable<ChargePartner>? ChargePartners { get; set; }
        public IEnumerable<ChargeMethod>? ChargeMethods { get; set; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }
        public SalesPeriod? Period { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}