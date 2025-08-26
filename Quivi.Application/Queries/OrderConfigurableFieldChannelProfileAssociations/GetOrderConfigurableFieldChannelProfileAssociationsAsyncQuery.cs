using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.OrderConfigurableFieldChannelProfileAssociations
{
    public class GetOrderConfigurableFieldChannelProfileAssociationsAsyncQuery : APagedAsyncQuery<OrderConfigurableFieldChannelProfileAssociation>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? OrderConfigurableFieldIds { get; init; }
        public IEnumerable<int>? ChannelProfileIds { get; init; }
    }

    public class GetOrderConfigurableFieldChannelProfileAssociationsAsyncQueryHandler : IQueryHandler<GetOrderConfigurableFieldChannelProfileAssociationsAsyncQuery, Task<IPagedData<OrderConfigurableFieldChannelProfileAssociation>>>
    {
        private readonly IOrderConfigurableFieldChannelProfileAssociationsRepository repository;

        public GetOrderConfigurableFieldChannelProfileAssociationsAsyncQueryHandler(IOrderConfigurableFieldChannelProfileAssociationsRepository repository)
        {
            this.repository = repository;
        }

        public Task<IPagedData<OrderConfigurableFieldChannelProfileAssociation>> Handle(GetOrderConfigurableFieldChannelProfileAssociationsAsyncQuery query)
        {
            return repository.GetAsync(new GetOrderConfigurableFieldChannelProfileAssociationsCriteria
            {
                MerchantIds = query.MerchantIds,
                ChannelProfileIds = query.ChannelProfileIds,
                OrderConfigurableFieldIds = query.OrderConfigurableFieldIds,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}