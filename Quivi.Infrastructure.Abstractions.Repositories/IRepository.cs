using Quivi.Domain.Entities;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface IRepository<T, TPagedCriteria> where T : class, IEntity where TPagedCriteria : IPagedCriteria
    {
        Task<IPagedData<T>> GetAsync(TPagedCriteria criteria);
        void Add(T entity);
        void Remove(T entity);

        Task SaveChangesAsync();
    }
}
