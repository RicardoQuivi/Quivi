using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetChannelProfilesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? PreparationGroupIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public ChannelFeature? Flags { get; init; }
        public bool? HasChannels { get; init; }
        public bool? IsDeleted { get; init; } = false;

        public bool IncludeChannels { get; init; }
        public bool IncludeAssociatedOrderConfigurableFields { get; init; }
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}