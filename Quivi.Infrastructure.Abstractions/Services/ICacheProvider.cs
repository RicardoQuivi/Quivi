namespace Quivi.Infrastructure.Abstractions.Services
{
    public interface ICacheProvider
    {
        /// <summary>
        /// Get an entry from cache.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The cache results indicates if exists and if exists then return the data.</returns>
        Task<CacheResult<T>> Get<T>(string key, string? context = null, bool extendExpiration = false);

        /// <summary>
        /// Set cache entry.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="context">The context to avoid conflicts.</param>
        /// <param name="ttl">Time to Live.</param>
        /// <returns><c>True</c> if success, otherwise <c>False</c>.</returns>
        Task<bool> Set<T>(string key, T value, string? context = null, TimeSpan? ttl = null);

        /// <summary>
        /// Try to get a entry from cache. If not exists then create.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="entryFactory">The method to create entry if not exists in cache yet.</param>
        /// <param name="context"></param>
        /// <param name="ttl">Time to Live.</param>
        /// <returns></returns>
        Task<T?> GetOrCreate<T>(string key, Func<Task<T>> entryFactory, string? context = null, TimeSpan? ttl = null, bool extendExpiration = false);
    }

    public class CacheResult<T>
    {
        public bool Exists { get; set; }
        public T? Data { get; set; }
    }
}