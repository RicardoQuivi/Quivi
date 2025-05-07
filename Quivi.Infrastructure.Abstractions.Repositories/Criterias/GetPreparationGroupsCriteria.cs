using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPreparationGroupsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; set; }
        public IEnumerable<int>? SessionIds { get; set; }
        public IEnumerable<int>? Ids { get; set; }
        public IEnumerable<PreparationGroupState>? States { get; set; }
        public IEnumerable<int>? ParentPreparationGroupIds { get; set; }
        public IEnumerable<int>? LocationIds { get; set; }
        public IEnumerable<int>? OrderIds { get; set; }
        public DateTime? FromCreatedDate { get; set; }
        public DateTime? ToCreatedDate { get; set; }
        public bool? Completed { get; set; }

        public bool IncludePreparationGroupItems { get; set; }
        public bool IncludeOrders { get; set; }
        public bool IncludePreparationGroupItemMenuItems { get; set; }
        public bool IncludeSession { get; set; }
        public bool IncludeSessionChannel { get; set; }
        public bool IncludeOrderFields { get; set; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}