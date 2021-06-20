﻿using System;
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
        public ICacheProvider CacheProvider { get; } = new MockCacheProvider();
        public CacheDefaults DefaultCachePolicy { get; set; } = new CacheDefaults();

        public T Get<T>(string key)
        {
            return default(T);
        }

        public T GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory)
        {
            return addItemFactory(new MockCacheEntry(key));
        }

        public T GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory, MemoryCacheEntryOptions policy)
        {
            return addItemFactory(new MockCacheEntry(key));
        }

        public Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory,
            MemoryCacheEntryOptions policy)
        {
            return addItemFactory(new MockCacheEntry(key));
        }

        public void Remove(string key)
        {
        }

        public Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory)
        {
            return addItemFactory(new MockCacheEntry(key));
        }

        public Task<T> GetAsync<T>(string key)
        {
            return Task.FromResult(default(T));
        }

        public void Add<T>(string key, T item, MemoryCacheEntryOptions policy)
        {
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            value = default(T);
            return true;
        }

        public bool TryGetValueAsync<T>(string key, out Task<T> value)
        {
            value = Task.FromResult(default(T));
            return true;
        }
    }
}