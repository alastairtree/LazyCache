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

        public object GetOrCreate<T>(string key, MemoryCacheEntryOptions policy, Func<ICacheEntry, T> func)
        {
            return func(null);
        }

        public void Remove(string key)
        {
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<ICacheEntry, Task<T>> func)
        {
            return func(null);
        }

        public bool TryGetValue<T>(object key, out T value)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public void Compact(double percentage)
        {
            
        }
    }
}