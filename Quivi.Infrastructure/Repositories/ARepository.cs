using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;

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
            return ToPagedDataAsync(query, criteria.PageIndex, criteria.PageSize);
        }

        private static async Task<IPagedData<T>> ToPagedDataAsync<T>(IOrderedQueryable<T> query, int page, int? pageSize)
        {
            if (pageSize == null)
            {
                var result = await query.ToListAsync();
                return new PagedData<T>(result)
                {
                    NumberOfPages = 1,
                    CurrentPage = 0,
                    TotalItems = result.Count,
                };
            }

            return await ToPagedDataAsync(query, page, pageSize ?? 10);
        }

        private static async Task<IPagedData<T>> ToPagedDataAsync<T>(IOrderedQueryable<T> query, int page, int pageSize)
        {
            int totalItems = await query.CountAsync();
            int numberOfPages = pageSize == 0 ? 0 : Convert.ToInt32(Math.Ceiling((double)totalItems / pageSize));

            return new PagedData<T>(pageSize == 0 ? new List<T>() : await query.Skip(pageSize * page).Take(pageSize).ToListAsync())
            {
                NumberOfPages = numberOfPages,
                CurrentPage = page,
                TotalItems = totalItems,
            };
        }

        public abstract IOrderedQueryable<TEntity> GetFilteredQueryable(TCriteria criteria);
    }
}
