using Quivi.Domain.Entities.Pos;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetOrderAdditionalInfosCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<int>? OrderConfigurableFieldIds { get; init; }
        public IEnumerable<PrintedOn>? PrintedOn { get; init; }
        public IEnumerable<AssignedOn>? AssignedOn { get; init; }
        public bool? IsAutoFill { get; init; }

        public bool IncludeOrderConfigurableField { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}