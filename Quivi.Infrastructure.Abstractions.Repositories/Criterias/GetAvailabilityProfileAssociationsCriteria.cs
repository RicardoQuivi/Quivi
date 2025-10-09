namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetAvailabilityProfileAssociationsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? AvailabilityGroupIds { get; init; }
        public IEnumerable<int>? ChannelProfileIds { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}