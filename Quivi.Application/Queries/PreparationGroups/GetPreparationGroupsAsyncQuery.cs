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
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<PreparationGroupState>? States { get; init; }
        public IEnumerable<int>? ParentPreparationGroupIds { get; init; }
        public IEnumerable<int>? LocationIds { get; init; }
        public IEnumerable<int>? OrderIds { get; init; }
        public DateTime? FromCreatedDate { get; init; }
        public DateTime? ToCreatedDate { get; init; }
        public bool? Completed { get; init; }

        public bool IncludePreparationGroupItems { get; init; }
        public bool IncludeOrders { get; init; }
        public bool IncludeOrdersSequence { get; init; }
        public bool IncludePreparationGroupItemMenuItems { get; init; }
        public bool IncludeSession { get; init; }
        public bool IncludeSessionChannel { get; init; }
        public bool IncludeSessionChannelProfile { get; init; }
        public bool IncludeOrderFields { get; init; }
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
                Ids = query.Ids,
                States = query.States,
                ParentPreparationGroupIds = query.ParentPreparationGroupIds,
                LocationIds = query.LocationIds,
                OrderIds = query.OrderIds,
                FromCreatedDate = query.FromCreatedDate,
                ToCreatedDate = query.ToCreatedDate,
                Completed = query.Completed,

                IncludePreparationGroupItems = query.IncludePreparationGroupItems,
                IncludeOrders = query.IncludeOrders,
                IncludeOrdersSequence = query.IncludeOrdersSequence,
                IncludePreparationGroupItemMenuItems = query.IncludePreparationGroupItemMenuItems,
                IncludeSession = query.IncludeSession,
                IncludeSessionChannel = query.IncludeSessionChannel,
                IncludeOrderFields = query.IncludeOrderFields,
                IncludeSessionChannelProfile = query.IncludeSessionChannelProfile,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
