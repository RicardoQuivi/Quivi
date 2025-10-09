namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetAvailabilityGroupsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public bool? AutoAddNewMenuItems { get; init; }
        public bool? AutoAddNewChannelProfiles { get; init; }

        public bool IncludeWeeklyAvailabilities { get; init; }
        public bool IncludeAssociatedChannelProfiles { get; init; }
        public bool IncludeAssociatedMenuItems { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}