using Microsoft.Extensions.Caching.Memory;
using Quivi.Infrastructure.Abstractions.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quivi.Infrastructure.Services
{
    public class MemoryCacheProvider : ICacheProvider
    {
        private readonly Lazy<IMemoryCache> _memoryCache;

        private class CacheEntry<T>
        {
            public CacheEntry(string jsonData, TimeSpan? expiration)
            {
                JsonData = jsonData;
                Expiration = expiration;
                _data = new Lazy<T?>(() => JsonSerializer.Deserialize<T?>(jsonData));
            }

            public CacheEntry(T? data, TimeSpan? expiration)
            {
                JsonData = JsonSerializer.Serialize(data);
                Expiration = expiration;
                _data = new Lazy<T?>(() => data);
            }

            public string JsonData { get; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public TimeSpan? Expiration { get; }

            [JsonIgnore]
            public T? Data => _data.Value;
            private readonly Lazy<T?> _data;
        }

        public MemoryCacheProvider()
        {
            _memoryCache = new Lazy<IMemoryCache>(() => new MemoryCache(new MemoryCacheOptions()));
        }

        public Task<CacheResult<T>> Get<T>(string key, string? context, bool extendExpiration)
        {
            try
            {
                string entryKey = BuildCompleteKey(key, context);
                if (!_memoryCache.Value.TryGetValue(entryKey, out CacheEntry<T>? entryValue) || entryValue == null)
                {
                    return Task.FromResult(new CacheResult<T>
                    {
                        Exists = false,
                    });
                }

                // Slide expiration each time someone gets the item
                if (extendExpiration)
                    SetCacheEntry(entryKey, entryValue);

                return Task.FromResult(new CacheResult<T>
                {
                    Exists = true,
                    Data = entryValue.Data,
                });
            }
            catch
            {
                return Task.FromResult(new CacheResult<T>
                {
                    Exists = false,
                });
            }
        }

        public async Task<T?> GetOrCreate<T>(string key, Func<Task<T>> entryFactory, string? context, TimeSpan? ttl, bool extendExpiration)
        {
            var result = await Get<T>(key, context, extendExpiration);
            T? entry = result.Data;

            if (result.Exists == false)
            {
                entry = await entryFactory();
                await Set(key, entry, context, ttl);
            }

            return entry;
        }

        public Task<bool> Set<T>(string key, T value, string? context, TimeSpan? ttl)
        {
            string entryKey = BuildCompleteKey(key, context);
            var entryValue = new CacheEntry<T>(value, ttl);

            bool success = SetCacheEntry(entryKey, entryValue);
            return Task.FromResult(success);
        }

        private string BuildCompleteKey(string key, string? context) => $"{context}#{key}";

        private bool SetCacheEntry<T>(string completeKey, CacheEntry<T> entry)
        {
            try
            {
                var options = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = entry.Expiration ?? TimeSpan.FromDays(300)
                };

                _memoryCache.Value.Set(completeKey, entry, options);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }

    public class NeverChacheProvider : ICacheProvider
    {
        public Task<CacheResult<T>> Get<T>(string key, string? context = null, bool extendExpiration = false) => Task.FromResult(new CacheResult<T>
        {
            Exists = false,
        });

        public async Task<T?> GetOrCreate<T>(string key, Func<Task<T>> entryFactory, string? context = null, TimeSpan? ttl = null, bool extendExpiration = false) => await entryFactory();

        public Task<bool> Set<T>(string key, T value, string? context = null, TimeSpan? ttl = null) => Task.FromResult(true);
    }
}