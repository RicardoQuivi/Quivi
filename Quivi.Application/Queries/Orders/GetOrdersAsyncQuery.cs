using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Orders
{
    public class GetOrdersAsyncQuery : APagedAsyncQuery<Order>
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<OrderState>? States { get; init; }

        public bool IncludeChannelProfilePosIntegration { get; init; }
        public bool IncludeOrderMenuItems { get; init; }
        public bool IncludeOrderMenuItemsPosChargeInvoiceItems { get; init; }
    }

    public class GetOrdersAsyncQueryHandler : APagedQueryAsyncHandler<GetOrdersAsyncQuery, Order>
    {
        private readonly IOrdersRepository repository;

        public GetOrdersAsyncQueryHandler(IOrdersRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Order>> Handle(GetOrdersAsyncQuery query)
        {
            return repository.GetAsync(new GetOrdersCriteria
            {
                MerchantIds = query.MerchantIds,
                Ids = query.Ids,
                SessionIds = query.SessionIds,
                States = query.States,

                IncludeChannelProfilePosIntegration = query.IncludeChannelProfilePosIntegration,
                IncludeOrderMenuItems = query.IncludeOrderMenuItems,
                IncludeOrderMenuItemsPosChargeInvoiceItems = query.IncludeOrderMenuItemsPosChargeInvoiceItems,

                PageSize = query.PageSize,
                PageIndex = query.PageIndex,
            });
        }
    }
}
