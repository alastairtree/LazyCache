﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache.Providers
{
    public class MemoryCacheProvider : ICacheProvider
    {
        internal readonly Func<IMemoryCache> cacheFactory;
        internal IMemoryCache cache;

        public MemoryCacheProvider(Func<IMemoryCache> cacheFactory)
        {
            this.cacheFactory = cacheFactory;
            cache = cacheFactory();
        }

        public void Set(string key, object item, MemoryCacheEntryOptions policy)
        {
            cache.Set(key, item, policy);
        }

        public object Get(string key)
        {
            return cache.Get(key);
        }

        public object GetOrCreate<T>(string key, Func<ICacheEntry, T> factory)
        {
            return cache.GetOrCreate(key, factory);
        }

        public void Remove(string key)
        {
            cache.Remove(key);
        }

        public void RemoveAll()
        {
            var oldCache = System.Threading.Interlocked.Exchange(ref cache, cacheFactory());
            oldCache?.Dispose();
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<ICacheEntry, Task<T>> factory)
        {
            return cache.GetOrCreateAsync(key, factory);
        }

        public void Dispose()
        {
            cache?.Dispose();
        }
    }
}