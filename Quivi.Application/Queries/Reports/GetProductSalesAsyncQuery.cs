using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Reports
{
    public class GetProductSalesAsyncQuery : APagedAsyncQuery<Domain.Repositories.EntityFramework.Models.ProductSales>
    {
        public SalesPeriod? Period { get; init; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }
        public ProductSalesSortBy SortBy { get; init; }
    }

    internal class GetProductSalesAsyncQueryHandler : APagedQueryAsyncHandler<GetProductSalesAsyncQuery, Domain.Repositories.EntityFramework.Models.ProductSales>
    {
        private readonly IReportsRepository repository;

        public GetProductSalesAsyncQueryHandler(IReportsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Domain.Repositories.EntityFramework.Models.ProductSales>> Handle(GetProductSalesAsyncQuery query)
        {
            return repository.GetProductSalesAsync(new GetProductSalesCriteria
            {
                Period = query.Period,
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                From = query.From,
                To = query.To,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
                SortBy = query.SortBy,
            });
        }
    }
}