namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetAvailabilityMenuItemAssociationsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? AvailabilityGroupIds { get; init; }
        public IEnumerable<int>? MenuItemIds { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}