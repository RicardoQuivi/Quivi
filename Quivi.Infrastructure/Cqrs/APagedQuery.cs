using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories.Data;

namespace Quivi.Infrastructure.Cqrs
{
    public class APagedQuery<T> : IQuery<IPagedData<T>>
    {
        public int PageNumber { get; init; } = 0;
        public int? PageSize { get; init; } = 0;
    }

    public class APagedAsyncQuery<T> : IQuery<Task<IPagedData<T>>>
    {
        public int PageIndex { get; init; } = 0;
        public int? PageSize { get; init; } = 0;
    }
}
