using System;
using System.Runtime.Caching;

namespace LazyCache.Mocks
{
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

        public void Add<T>(string key, DateTime expires, T item)
        {
        }
    }
}