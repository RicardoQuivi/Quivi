using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Infrastructure.Repositories
{
    public abstract class ARepository<TEntity, TCriteria> : IRepository<TEntity, TCriteria> where TEntity : class, IBaseEntity where TCriteria : IPagedCriteria
    {
        protected QuiviContext Context { get; }
        protected DbSet<TEntity> Set { get; }

        public ARepository(QuiviContext context)
        {
            this.Context = context;
            this.Set = context.Set<TEntity>();
        }

        public void Add(TEntity entity) => Set.Add(entity);

        public void Remove(TEntity entity) => Set.Remove(entity);
        public Task SaveChangesAsync() => Context.SaveChangesAsync();

        public Task<IPagedData<TEntity>> GetAsync(TCriteria criteria)
        {
            IOrderedQueryable<TEntity> query = GetFilteredQueryable(criteria);
            return query.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);
        }

        public abstract IOrderedQueryable<TEntity> GetFilteredQueryable(TCriteria criteria);
    }
}