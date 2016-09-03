using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace LazyCache.Mocks
{
    /// <summary>
    /// A mock implementation IAppCache that does not do any caching. 
    /// Useful in unit tests or for feature switching to swap in a dependency to disable all caching
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

        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, CacheItemPolicy policy)
        {
            return addItemFactory.Invoke();
        }

        public ObjectCache ObjectCache => null;


        public void Add<T>(string key, T item, TimeSpan slidingExpiration)
        {
        }

        public void Add<T>(string key, T item, CacheItemPolicy policy)
        {
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, TimeSpan slidingExpiration)
        {
            return addItemFactory.Invoke();
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, CacheItemPolicy policy)
        {
            return addItemFactory.Invoke();
        }
    }
}