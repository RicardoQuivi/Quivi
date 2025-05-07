using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Commands.Sessions
{
    public class GetSessionsAsyncQuery : APagedAsyncQuery<Session>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<SessionStatus>? Statuses { get; init; }
        public bool LatestSessionsOnly { get; init; }
        public bool IncludeOrdersMenuItems { get; init; }
        public bool IncludeOrdersMenuItemsPosChargeInvoiceItems { get; init; }
        public bool IncludeOrdersMenuItemsModifiers { get; init; }
        public bool IncludeChannel { get; init; }
    }

    public class GetSessionsAsyncQueryHandler : APagedQueryAsyncHandler<GetSessionsAsyncQuery, Session>
    {
        private readonly ISessionsRepository repository;

        public GetSessionsAsyncQueryHandler(ISessionsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Session>> Handle(GetSessionsAsyncQuery query)
        {
            return repository.GetAsync(new GetSessionsCriteria
            {
                MerchantIds = query.MerchantIds,
                Ids = query.Ids,
                ChannelIds = query.ChannelIds,
                Statuses = query.Statuses,

                IncludeOrdersMenuItems = query.IncludeOrdersMenuItems,
                IncludeOrdersMenuItemsPosChargeInvoiceItems = query.IncludeOrdersMenuItemsPosChargeInvoiceItems,
                IncludeChannel = query.IncludeChannel,
                IncludeOrdersMenuItemsModifiers = query.IncludeOrdersMenuItemsModifiers,

                PageSize = query.PageSize,
                PageIndex = query.PageIndex,
            });
        }
    }
}
