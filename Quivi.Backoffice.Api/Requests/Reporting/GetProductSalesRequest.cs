using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Backoffice.Api.Requests.Reporting
{
    public class GetProductSalesRequest : APagedRequest
    {
        public bool? AdminView { get; init; }
        public SalesPeriod? Period { get; init; }
        public ProductSalesSortBy SortBy { get; init; }
        public DateTimeOffset? From { get; init; }
        public DateTimeOffset? To { get; init; }
    }
}