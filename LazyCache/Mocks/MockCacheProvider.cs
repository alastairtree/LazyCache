using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache.Mocks
{
    public class MockCacheProvider : ICacheProvider
    {
        public void Set(string key, object item, MemoryCacheEntryOptions policy)
        {
        }

        public object Get(string key)
        {
            return null;
        }

        public object GetOrCreate<T>(string key, Func<ICacheEntry, T> func)
        {
            return func(null);
        }

        public void Remove(string key)
        {
        }

        public void RemoveAll()
        {
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<ICacheEntry, Task<T>> func)
        {
            return func(null);
        }

        public void Dispose()
        {
        }
    }
}