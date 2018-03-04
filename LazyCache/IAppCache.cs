using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache
{
    public interface IAppCache
    {
        ICacheProvider CacheProvider { get; }

        void Add<T>(string key, T item);
        void Add<T>(string key, T item, DateTimeOffset absoluteExpiration);
        void Add<T>(string key, T item, TimeSpan slidingExpiration);
        void Add<T>(string key, T item, MemoryCacheEntryOptions policy);

        T Get<T>(string key);

        T GetOrAdd<T>(string key, Func<T> addItemFactory);
        T GetOrAdd<T>(string key, Func<T> addItemFactory, DateTimeOffset absoluteExpiration);
        T GetOrAdd<T>(string key, Func<T> addItemFactory, TimeSpan slidingExpiration);
        T GetOrAdd<T>(string key, Func<T> addItemFactory, MemoryCacheEntryOptions policy);
        T GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory);

        void Remove(string key);

        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory);
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, MemoryCacheEntryOptions policy);
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, DateTimeOffset expires);
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, TimeSpan slidingExpiration);
        Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory);

        Task<T> GetAsync<T>(string key);
    }
}