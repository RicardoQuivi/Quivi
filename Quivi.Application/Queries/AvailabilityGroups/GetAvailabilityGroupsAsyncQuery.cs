using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.AvailabilityGroups
{
    public class GetAvailabilityGroupsAsyncQuery : APagedAsyncQuery<AvailabilityGroup>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public bool? AutoAddNewMenuItems { get; init; }
        public bool? AutoAddNewChannelProfiles { get; init; }

        public bool IncludeWeeklyAvailabilities { get; init; }
    }

    public class GetAvailabilityGroupsAsyncQueryHandler : APagedQueryAsyncHandler<GetAvailabilityGroupsAsyncQuery, AvailabilityGroup>
    {
        private readonly IAvailabilityGroupsRepository repository;

        public GetAvailabilityGroupsAsyncQueryHandler(IAvailabilityGroupsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<AvailabilityGroup>> Handle(GetAvailabilityGroupsAsyncQuery query)
        {
            return repository.GetAsync(new GetAvailabilityGroupsCriteria
            {
                MerchantIds = query.MerchantIds,
                Ids = query.Ids,
                AutoAddNewMenuItems = query.AutoAddNewMenuItems,
                AutoAddNewChannelProfiles = query.AutoAddNewChannelProfiles,

                IncludeWeeklyAvailabilities = query.IncludeWeeklyAvailabilities,

                PageSize = query.PageSize,
                PageIndex = query.PageIndex,
            });
        }
    }
}