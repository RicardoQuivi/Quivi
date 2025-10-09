namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetWeeklyAvailabilitiesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? ChannelProfileIds { get; init; }
        public IEnumerable<int>? MenuItemIds { get; init; }
        public IEnumerable<DateTime>? ChangesOnDate { get; init; }

        public bool IncludeAvailabilityGroup { get; init; }
        public bool IncludeAvailabilityGroupAssociatedMenuItems { get; init; }
        public bool IncludeAvailabilityGroupAssociatedChannelProfiles { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}