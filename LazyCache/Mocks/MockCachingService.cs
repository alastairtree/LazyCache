using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache.Mocks
{
    /// <summary>
    ///     A mock implementation IAppCache that does not do any caching.
    ///     Useful in unit tests or for feature switching to swap in a dependency to disable all caching
    /// </summary>
    public class MockCachingService : IAppCache
    {
        public void Add<T>(string key, T item)
        {
        }

        public void Add<T>(string key, T item, DateTimeOffset expires)
        {
        }

        public T Get<T>(string key)
        {
            return default(T);
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory)
        {
            return addItemFactory.Invoke();
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, DateTimeOffset expires)
        {
            return addItemFactory.Invoke();
        }

        public void Remove(string key)
        {
        }

        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, MemoryCacheEntryOptions policy)
        {
            return addItemFactory.Invoke();
        }

        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory)
        {
            return addItemFactory.Invoke();
        }

        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, DateTimeOffset expires)
        {
            return addItemFactory.Invoke();
        }

        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, TimeSpan slidingExpiration)
        {
            return addItemFactory.Invoke();
        }

        public Task<T> GetAsync<T>(string key)
        {
            return Task.FromResult(default(T));
        }

        public IMemoryCache MemoryCache => null;


        public void Add<T>(string key, T item, TimeSpan slidingExpiration)
        {
        }

        public void Add<T>(string key, T item, MemoryCacheEntryOptions policy)
        {
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, TimeSpan slidingExpiration)
        {
            return addItemFactory.Invoke();
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, MemoryCacheEntryOptions policy)
        {
            return addItemFactory.Invoke();
        }
    }
}