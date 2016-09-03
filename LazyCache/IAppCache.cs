using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace LazyCache
{
    public interface IAppCache
    {
        ObjectCache ObjectCache { get; }
        void Add<T>(string key, T item);
        void Add<T>(string key, T item, DateTimeOffset absoluteExpiration);
        void Add<T>(string key, T item, TimeSpan slidingExpiration);
        void Add<T>(string key, T item, CacheItemPolicy policy);

        T Get<T>(string key);

        T GetOrAdd<T>(string key, Func<T> addItemFactory);
        T GetOrAdd<T>(string key, Func<T> addItemFactory, DateTimeOffset absoluteExpiration);
        T GetOrAdd<T>(string key, Func<T> addItemFactory, TimeSpan slidingExpiration);
        T GetOrAdd<T>(string key, Func<T> addItemFactory, CacheItemPolicy policy);

        void Remove(string key);
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, CacheItemPolicy policy);
    }
}