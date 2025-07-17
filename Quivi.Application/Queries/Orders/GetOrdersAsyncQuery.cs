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
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<int>? ChargeIds { get; init; }
        public IEnumerable<int>? OrderMenuItemIds { get; init; }
        public IEnumerable<OrderState>? States { get; init; }
        public IEnumerable<OrderOrigin>? Origins { get; init; }

        public bool IncludeMerchant { get; init; }
        public bool IncludeOrderAdditionalFields { get; init; }
        public bool IncludeChangeLogs { get; init; }
        public bool IncludeOrderSequence { get; init; }
        public bool IncludeChannelProfile { get; init; }
        public bool IncludeChannelProfilePosIntegration { get; init; }
        public bool IncludeOrderMenuItems { get; init; }
        public bool IncludeOrderMenuItemsPosChargeInvoiceItems { get; init; }
        public bool IncludeOrderMenuItemsPosChargeInvoiceItemsPosCharge { get; init; }
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
                ChannelIds = query.ChannelIds,
                SessionIds = query.SessionIds,
                ChargeIds = query.ChargeIds,
                States = query.States,
                Origins = query.Origins,
                OrderMenuItemIds = query.OrderMenuItemIds,

                IncludeOrderSequence = query.IncludeOrderSequence,
                IncludeMerchant = query.IncludeMerchant,
                IncludeOrderAdditionalFields = query.IncludeOrderAdditionalFields,
                IncludeChannelProfile = query.IncludeChannelProfile,
                IncludeChannelProfilePosIntegration = query.IncludeChannelProfilePosIntegration,
                IncludeOrderMenuItems = query.IncludeOrderMenuItems,
                IncludeOrderMenuItemsPosChargeInvoiceItems = query.IncludeOrderMenuItemsPosChargeInvoiceItems,
                IncludeOrderMenuItemsPosChargeInvoiceItemsPosCharge = query.IncludeOrderMenuItemsPosChargeInvoiceItemsPosCharge,
                IncludeChangeLogs = query.IncludeChangeLogs,

                PageSize = query.PageSize,
                PageIndex = query.PageIndex,
            });
        }
    }
}
