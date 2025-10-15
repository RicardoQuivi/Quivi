using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Reports
{
    public class GetCategorySalesAsyncQuery : APagedAsyncQuery<Domain.Repositories.EntityFramework.Models.CategorySales>
    {
        public SalesPeriod? Period { get; init; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }
        public ProductSalesSortBy SortBy { get; init; }
    }

    internal class GetCategorySalesAsyncQueryHandler : APagedQueryAsyncHandler<GetCategorySalesAsyncQuery, Domain.Repositories.EntityFramework.Models.CategorySales>
    {
        private readonly IReportsRepository repository;

        public GetCategorySalesAsyncQueryHandler(IReportsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Domain.Repositories.EntityFramework.Models.CategorySales>> Handle(GetCategorySalesAsyncQuery query)
        {
            return repository.GetCategorySalesAsync(new GetCategorySalesCriteria
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