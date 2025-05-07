using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.OrderMenuItems
{
    public class GetOrderMenuItemsAsyncQuery : APagedAsyncQuery<OrderMenuItem>
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }

        public bool IncludeMenuItem { get; init; }
    }

    public class GetOrderMenuItemsAsyncQueryHandler : APagedQueryAsyncHandler<GetOrderMenuItemsAsyncQuery, OrderMenuItem>
    {
        private readonly IOrderMenuItemsRepository repository;

        public GetOrderMenuItemsAsyncQueryHandler(IOrderMenuItemsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<OrderMenuItem>> Handle(GetOrderMenuItemsAsyncQuery query)
        {
            return repository.GetAsync(new GetOrderMenuItemsCriteria
            {
                MerchantIds = query.MerchantIds,
                Ids = query.Ids,
                SessionIds = query.SessionIds,

                IncludeMenuItem = query.IncludeMenuItem,

                PageSize = query.PageSize,
                PageIndex = query.PageIndex,
            });
        }
    }
}
