using Quivi.Domain.Entities.Charges;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Backoffice.Api.Requests.Reporting
{
    public class GetPartnerChargeMethodSalesRequest : APagedRequest
    {
        public bool? AdminView { get; init; }
        public IEnumerable<ChargeMethod>? ChargeMethods { get; init; }
        public IEnumerable<ChargePartner>? ChargePartners { get; init; }
        public SalesPeriod? Period { get; init; }
        public ProductSalesSortBy SortBy { get; init; }
        public DateTimeOffset? From { get; init; }
        public DateTimeOffset? To { get; init; }
    }
}