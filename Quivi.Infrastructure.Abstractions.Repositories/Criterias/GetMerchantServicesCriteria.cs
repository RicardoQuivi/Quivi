using Quivi.Domain.Entities.Merchants;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetMerchantServicesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? PersonIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<MerchantServiceType>? MerchantServiceTypes { get; init; }

        public bool? UnpaidOnly { get; init; }

        public DateTime? PaymentsFrom { get; init; }
        public DateTime? PaymentsTo { get; init; }

        public bool IncludeMerchants { get; init; }
        public bool IncludePostings { get; init; }
        public bool IncludeJournals { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
