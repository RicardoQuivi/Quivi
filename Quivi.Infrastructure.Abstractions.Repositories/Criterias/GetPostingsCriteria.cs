namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPostingsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? PersonIds { get; init; }
        public IEnumerable<string>? JournalOrderRefs { get; init; }
        public DateTime? SettlementStartDate { get; init; }
        public DateTime? SettlementEndDate { get; init; }

        public bool IncludeJournal { get; init; }
        public bool IncludeJournalSettlementServiceDetails { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}