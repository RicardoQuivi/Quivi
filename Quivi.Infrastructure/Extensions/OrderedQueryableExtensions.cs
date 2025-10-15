using Microsoft.EntityFrameworkCore;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Repositories;

namespace Quivi.Infrastructure.Extensions
{
    public static class OrderedQueryableExtensions
    {
        public static async Task<IPagedData<T>> ToPagedDataAsync<T>(this IOrderedQueryable<T> query, int page, int? pageSize)
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

        public static async Task<IPagedData<T>> ToPagedDataAsync<T>(this IOrderedQueryable<T> query, int page, int pageSize)
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
    }
}