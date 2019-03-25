using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Threading.Tasks;


namespace LazyCache.Providers
{
    public class HybridCacheProvider : IDistributedCacheProvider
    {
        private readonly IDistributedCacheProvider distributedCacheProvider;
        private readonly IMemoryCache memoryCache;

        public HybridCacheProvider(IDistributedCacheProvider distributedCacheProvider, IMemoryCache memoryCache)
        {
            this.distributedCacheProvider = distributedCacheProvider;
            this.memoryCache = memoryCache;
        }

        public void Set(string key, object item, DistributedCacheEntryOptions policy)
        {
            distributedCacheProvider.Set(key, item, policy);
        }

        public T Get<T>(string key)
        {
            return distributedCacheProvider.Get<T>(key);
        }

        public object Get(string key)
        {
            return distributedCacheProvider.Get(key);
        }

        public object GetOrCreate<T>(string key, Func<DistributedCacheEntry, T> func)
        {
            if (!TryGetValue(key, out T result))
            {
                return memoryCache.GetOrCreate(key, (e) => func(new DistributedCacheEntry(key)));
            }

            return result;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<DistributedCacheEntry, Task<T>> func)
        {
            if (!TryGetValue(key, out T result))
            {
                return await memoryCache.GetOrCreateAsync(key, (e) => func(new DistributedCacheEntry(key)));
            }

            return result;
        }

        public void Remove(string key)
        {
            distributedCacheProvider.Remove(key);
        }

        private bool TryGetValue<T>(string key, out T value)
        {
            value = Get<T>(key);
            return value != null && !value.Equals(default(T));
        }
    }
}