using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public class GetChannelsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? ChannelProfileIds { get; init; }
        public IEnumerable<string>? Identifiers { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public ChannelFeature? Flags { get; init; }
        public string? Search { get; set; }
        public bool? HasOpenSession { get; set; }
        public bool? IsDeleted { get; init; } = false;

        public bool IncludeChannelProfile { get; set; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
