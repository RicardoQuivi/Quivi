using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.WeeklyAvailabilities
{
    public class GetWeeklyAvailabilitiesAsyncQuery : APagedAsyncQuery<WeeklyAvailability>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? ChannelProfileIds { get; init; }
        public IEnumerable<int>? MenuItemIds { get; init; }
        public IEnumerable<DateTime>? ChangesOnDate { get; init; }

        public bool IncludeAvailabilityGroup { get; init; }
        public bool IncludeAvailabilityGroupAssociatedMenuItems { get; init; }
        public bool IncludeAvailabilityGroupAssociatedChannelProfiles { get; init; }
    }

    public class GetWeeklyAvailabilitiesAsyncQueryHandler : IQueryHandler<GetWeeklyAvailabilitiesAsyncQuery, Task<IPagedData<WeeklyAvailability>>>
    {
        private readonly IWeeklyAvailabilitiesRepository repository;

        public GetWeeklyAvailabilitiesAsyncQueryHandler(IWeeklyAvailabilitiesRepository repository)
        {
            this.repository = repository;
        }

        public Task<IPagedData<WeeklyAvailability>> Handle(GetWeeklyAvailabilitiesAsyncQuery query)
        {
            return repository.GetAsync(new GetWeeklyAvailabilitiesCriteria
            {
                MerchantIds = query.MerchantIds,
                ChannelProfileIds = query.ChannelProfileIds,
                MenuItemIds = query.MenuItemIds,
                ChangesOnDate = query.ChangesOnDate,

                IncludeAvailabilityGroup = query.IncludeAvailabilityGroup,
                IncludeAvailabilityGroupAssociatedChannelProfiles = query.IncludeAvailabilityGroupAssociatedChannelProfiles,
                IncludeAvailabilityGroupAssociatedMenuItems = query.IncludeAvailabilityGroupAssociatedMenuItems,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
