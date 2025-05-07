namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public interface IPagedCriteria : ICriteria
    {
        int PageIndex { get; }
        int? PageSize { get; }
    }
}
