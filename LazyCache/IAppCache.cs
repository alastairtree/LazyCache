using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;

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
        Task<T> GetAsync<T>(string key);
        bool TryGetValue<T>(string key, out T value);

        [SuppressMessage("Naming", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "Using Async suffix here it's fine. This method name follows the same pattern used for the other methods in this interface.")]
        bool TryGetValueAsync<T>(string key, out Task<T> value);

        T GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory);
        T GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory, MemoryCacheEntryOptions policy);
        Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory);
        Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory, MemoryCacheEntryOptions policy);
        void Remove(string key);
    }
}