using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories.Data;

namespace Quivi.Infrastructure.Cqrs
{
    public abstract class APagedQueryHandler<TQuery, TData> : IQueryHandler<TQuery, IPagedData<TData>> where TQuery : IQuery<IPagedData<TData>>
    {
        public abstract IPagedData<TData> Handle(TQuery query);
    }

    public abstract class APagedQueryAsyncHandler<TQuery, TData> : IQueryHandler<TQuery, Task<IPagedData<TData>>> where TQuery : IQuery<Task<IPagedData<TData>>>
    {
        public abstract Task<IPagedData<TData>> Handle(TQuery query);
    }
}
