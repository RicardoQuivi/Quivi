using Quivi.Domain.Entities.Financing;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetJournalsCriteria : IPagedCriteria
    {
        public IEnumerable<JournalState>? States { get; init; }
        public IEnumerable<JournalType>? Types { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public IEnumerable<string>? OrderRefs { get; init; }

        public bool IncludeJournalDetails { get; init; }
        public bool IncludeMerchantFees { get; init; }
        public bool IncludeSubMerchantFees { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}