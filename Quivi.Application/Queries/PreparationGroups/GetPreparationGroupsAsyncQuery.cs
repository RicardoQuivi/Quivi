using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.PreparationGroups
{
    public class GetPreparationGroupsAsyncQuery : APagedAsyncQuery<PreparationGroup>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<int>? LocationIds { get; init; }
        public IEnumerable<int>? OrderIds { get; init; }
        public IEnumerable<PreparationGroupState>? States { get; init; }
        public DateTime? FromCreatedDate { get; init; }
        public DateTime? ToCreatedDate { get; init; }
        public bool? Completed { get; init; }
        public bool IncludeOrders { get; init; }
        public bool IncludePreparationGroupItems { get; init; }
        public bool IncludePreparationGroupItemMenuItems { get; init; }
    }

    public class GetPreparationGroupsAsyncQueryHandler : APagedQueryAsyncHandler<GetPreparationGroupsAsyncQuery, PreparationGroup>
    {
        private readonly IPreparationGroupsRepository repository;

        public GetPreparationGroupsAsyncQueryHandler(IPreparationGroupsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<PreparationGroup>> Handle(GetPreparationGroupsAsyncQuery query)
        {
            return repository.GetAsync(new Infrastructure.Abstractions.Repositories.Criterias.GetPreparationGroupsCriteria
            {
                MerchantIds = query.MerchantIds,
                SessionIds = query.SessionIds,
                LocationIds = query.LocationIds,
                OrderIds = query.OrderIds,
                States = query.States,
                Completed = query.Completed,
                FromCreatedDate = query.FromCreatedDate,
                ToCreatedDate = query.ToCreatedDate,
                IncludeOrders = query.IncludeOrders,
                IncludePreparationGroupItems = query.IncludePreparationGroupItems,
                IncludePreparationGroupItemMenuItems = query.IncludePreparationGroupItemMenuItems,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
