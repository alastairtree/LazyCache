using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace LazyCache
{
    public interface IDistributedAppCache
    {
        IDistributedCacheProvider DistributedCacheProvider { get; }

        /// <summary>
        ///     Define the number of seconds to cache objects for by default
        /// </summary>
        DistributedCacheDefaults DefaultCachePolicy { get; }

        void Add<T>(string key, T item, DistributedCacheEntryOptions policy);

        T Get<T>(string key);

        T GetOrAdd<T>(string key, Func<DistributedCacheEntry, T> addItemFactory);

        Task<T> GetAsync<T>(string key);

        Task<T> GetOrAddAsync<T>(string key, Func<DistributedCacheEntry, Task<T>> addItemFactory);

        void Remove(string key);
    }
}