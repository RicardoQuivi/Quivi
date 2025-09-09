using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.OrderConfigurableFields
{
    public class GetOrderConfigurableFieldsAsyncQuery : APagedAsyncQuery<OrderConfigurableField>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? ChannelProfileIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<string>? Names { get; init; }
        public bool? ForPosSessions { get; init; }
        public bool? ForOrdering { get; init; }
        public bool? IsAutoFill { get; init; }
        public bool? IsDeleted { get; init; }
        public bool IncludeTranslations { get; init; }
    }

    public class GetOrderConfigurableFieldsAsyncQueryHandler : IQueryHandler<GetOrderConfigurableFieldsAsyncQuery, Task<IPagedData<OrderConfigurableField>>>
    {
        private readonly IOrderConfigurableFieldsRepository repository;

        public GetOrderConfigurableFieldsAsyncQueryHandler(IOrderConfigurableFieldsRepository repository)
        {
            this.repository = repository;
        }

        public Task<IPagedData<OrderConfigurableField>> Handle(GetOrderConfigurableFieldsAsyncQuery query)
        {
            return repository.GetAsync(new GetOrderConfigurableFieldsCriteria
            {
                Ids = query.Ids,
                MerchantIds = query.MerchantIds,
                ChannelIds = query.ChannelIds,
                ChannelProfileIds = query.ChannelProfileIds,
                ForPosSessions = query.ForPosSessions,
                ForOrdering = query.ForOrdering,
                IsAutoFill = query.IsAutoFill,
                IsDeleted = query.IsDeleted,
                IncludeTranslations = query.IncludeTranslations,
                Names = query.Names,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}