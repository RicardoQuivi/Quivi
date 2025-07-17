using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPreparationGroupsCriteria : IPagedCriteria
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

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}