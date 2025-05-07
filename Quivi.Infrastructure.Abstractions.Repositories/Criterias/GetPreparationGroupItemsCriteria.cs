namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPreparationGroupItemsCriteria : IPagedCriteria
    {
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}