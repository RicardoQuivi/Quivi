using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Reports
{
    public class GetChargeMethodSalesAsyncQuery : APagedAsyncQuery<Domain.Repositories.EntityFramework.Models.ChargeMethodSales>
    {
        public SalesPeriod? Period { get; init; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }
        public ProductSalesSortBy SortBy { get; init; }
    }

    internal class GetChargeMethodSalesAsyncQueryHandler : APagedQueryAsyncHandler<GetChargeMethodSalesAsyncQuery, Domain.Repositories.EntityFramework.Models.ChargeMethodSales>
    {
        private readonly IReportsRepository repository;

        public GetChargeMethodSalesAsyncQueryHandler(IReportsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Domain.Repositories.EntityFramework.Models.ChargeMethodSales>> Handle(GetChargeMethodSalesAsyncQuery query)
        {
            return repository.GetChargeMethodSalesAsync(new GetChargeMethodSalesCriteria
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