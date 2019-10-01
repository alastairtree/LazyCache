using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache
{
    public interface IAppCache
    {
        ICacheProvider CacheProvider { get; }

        /// <summary>
        ///     Define the number of seconds to cache objects for by default
        /// </summary>
        CacheDefaults DefaultCachePolicy { get; }

        void Add<T>(string key, T item, MemoryCacheEntryOptions policy);

        T Get<T>(string key);

        T GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory);

        Task<T> GetAsync<T>(string key);

        Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory);

        void Remove(string key);

        void RemoveAll();
    }
}