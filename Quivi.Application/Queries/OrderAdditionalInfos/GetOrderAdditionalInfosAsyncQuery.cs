using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.OrderAdditionalInfos
{
    public class GetOrderAdditionalInfosAsyncQuery : APagedAsyncQuery<OrderAdditionalInfo>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<PrintedOn>? PrintedOn { get; init; }
        public IEnumerable<AssignedOn>? AssignedOn { get; init; }
        public bool? IsAutoFill { get; init; }

        public bool IncludeOrderConfigurableField { get; init; }
    }

    public class GetOrderAdditionalInfosAsyncQueryHandler : APagedQueryAsyncHandler<GetOrderAdditionalInfosAsyncQuery, OrderAdditionalInfo>
    {
        private readonly IOrderAdditionalInfosRepository repository;

        public GetOrderAdditionalInfosAsyncQueryHandler(IOrderAdditionalInfosRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<OrderAdditionalInfo>> Handle(GetOrderAdditionalInfosAsyncQuery query)
        {
            return repository.GetAsync(new GetOrderAdditionalInfosCriteria
            {
                MerchantIds = query.MerchantIds,
                SessionIds = query.SessionIds,
                PrintedOn = query.PrintedOn,
                IsAutoFill = query.IsAutoFill,

                IncludeOrderConfigurableField = query.IncludeOrderConfigurableField,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}