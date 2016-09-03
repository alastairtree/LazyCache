using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace LazyCache
{
    public class CachingService : IAppCache
    {
        private readonly ObjectCache cache;

        public CachingService() : this(MemoryCache.Default)
        {
        }

        public CachingService(ObjectCache cache)
        {
            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }
            this.cache = cache;
            DefaultCacheDuration = 60*20;
        }

        /// <summary>
        ///     Seconds to cache objects for by default
        /// </summary>
        public int DefaultCacheDuration { get; set; }

        private DateTimeOffset DefaultExpiryDateTime => DateTimeOffset.Now.AddSeconds(DefaultCacheDuration);

        public void Add<T>(string key, T item)
        {
            Add(key, item, DefaultExpiryDateTime);
        }

        public void Add<T>(string key, T item, DateTimeOffset expires)
        {
            Add(key, item, new CacheItemPolicy {AbsoluteExpiration = expires});
        }

        public void Add<T>(string key, T item, TimeSpan slidingExpiration)
        {
            Add(key, item, new CacheItemPolicy {SlidingExpiration = slidingExpiration});
        }

        public void Add<T>(string key, T item, CacheItemPolicy policy)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            ValidateKey(key);

            cache.Set(key, item, policy);
        }

        public T Get<T>(string key)
        {
            ValidateKey(key);

            var item = cache[key];

            if (item is T)
            {
                return (T) item;
            }

            var lazy = item as Lazy<T>;
            return lazy != null ? lazy.Value : default(T);
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory)
        {
            return GetOrAdd(key, addItemFactory, DefaultExpiryDateTime);
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory)
        {
            return await GetOrAddAsync(key, addItemFactory, DefaultExpiryDateTime);
        }


        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, CacheItemPolicy policy)
        {
            ValidateKey(key);

            var newLazyCacheItem = new AsyncLazy<T>(addItemFactory);

            EnsureRemovedCallbackDoesNotReturnTheAsyncLazy<T>(policy);

            var existingCacheItem = cache.AddOrGetExisting(key, newLazyCacheItem, policy);

            if (existingCacheItem != null)
            {
                var asyncLazy = existingCacheItem as AsyncLazy<T>;
                if (asyncLazy != null)
                {
                    return await asyncLazy.Value;
                }

                var task = existingCacheItem as Task<T>;
                if (task != null)
                {
                    return await task;
                }

                if (existingCacheItem is T)
                {
                    return (T) existingCacheItem;
                }

                return default(T);
            }

            try
            {
                var result = newLazyCacheItem.Value;

                if(result.IsCanceled || result.IsFaulted)
                    cache.Remove(key);

                return await result;
            }
            catch //addItemFactory errored so do not cache the exception
            {
                cache.Remove(key);
                throw;
            }
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, DateTimeOffset expires)
        {
            return GetOrAdd(key, addItemFactory, new CacheItemPolicy {AbsoluteExpiration = expires});
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, DateTimeOffset expires)
        {
            return await GetOrAddAsync(key, addItemFactory, new CacheItemPolicy { AbsoluteExpiration = expires });
        }


        public T GetOrAdd<T>(string key, Func<T> addItemFactory, TimeSpan slidingExpiration)
        {
            return GetOrAdd(key, addItemFactory, new CacheItemPolicy {SlidingExpiration = slidingExpiration});
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, TimeSpan slidingExpiration)
        {
            return await GetOrAddAsync(key, addItemFactory, new CacheItemPolicy { SlidingExpiration = slidingExpiration });
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, CacheItemPolicy policy)
        {

            ValidateKey(key);

            var newLazyCacheItem = new Lazy<T>(addItemFactory);

            EnsureRemovedCallbackDoesNotReturnTheLazy<T>(policy);

            var existingCacheItem = cache.AddOrGetExisting(key, newLazyCacheItem, policy);

            if (existingCacheItem != null)
            {
                if (existingCacheItem is T)
                {
                    return (T) existingCacheItem;
                }

                if (existingCacheItem is Lazy<T>)
                {
                    return ((Lazy<T>) existingCacheItem).Value;
                }

                return default(T);
            }

            try
            {
                return newLazyCacheItem.Value;
            }
            catch //addItemFactory errored so do not cache the exception
            {
                cache.Remove(key);
                throw;
            }
        }


        public void Remove(string key)
        {
            ValidateKey(key);
            cache.Remove(key);
        }

        public ObjectCache ObjectCache => cache;

        private static void EnsureRemovedCallbackDoesNotReturnTheLazy<T>(CacheItemPolicy policy)
        {
            if (policy != null && policy.RemovedCallback != null)
            {
                var originallCallback = policy.RemovedCallback;
                policy.RemovedCallback = args =>
                {
                    //unwrap the cache item in a callback given one is specified
                    if (args != null && args.CacheItem != null)
                    {
                        var item = args.CacheItem.Value as Lazy<T>;
                        if (item != null)
                        {
                            args.CacheItem.Value = item.IsValueCreated ? item.Value : default(T);
                        }
                    }
                    originallCallback(args);
                };
            }
        }

        private static void EnsureRemovedCallbackDoesNotReturnTheAsyncLazy<T>(CacheItemPolicy policy)
        {
            if (policy != null && policy.RemovedCallback != null)
            {
                var originallCallback = policy.RemovedCallback;
                policy.RemovedCallback = args =>
                {
                    //unwrap the cache item in a callback given one is specified
                    if (args != null && args.CacheItem != null)
                    {
                        var item = args.CacheItem.Value as AsyncLazy<T>;
                        if (item != null)
                        {
                            args.CacheItem.Value = item.IsValueCreated ? item.Value : Task.FromResult(default(T));
                        }
                    }
                    originallCallback(args);
                };
            }
        }

        private void ValidateKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentOutOfRangeException(nameof(key), @"Cache keys cannot be empty or whitespace");
            }
        }
    }
}