using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Reports
{
    public class GetSalesAsyncQuery : APagedAsyncQuery<Domain.Repositories.EntityFramework.Models.Sales>
    {
        public SalesPeriod? Period { get; init; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }
    }

    public class GetSalesAsyncQueryHandler : APagedQueryAsyncHandler<GetSalesAsyncQuery, Domain.Repositories.EntityFramework.Models.Sales>
    {
        private readonly IReportsRepository repository;

        public GetSalesAsyncQueryHandler(IReportsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Domain.Repositories.EntityFramework.Models.Sales>> Handle(GetSalesAsyncQuery query)
        {
            return repository.GetSalesAsync(new GetSalesCriteria
            {
                Period = query.Period,

                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                From = query.From,
                To = query.To,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}