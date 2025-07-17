namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetJournalsCriteria : IPagedCriteria
    {
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}