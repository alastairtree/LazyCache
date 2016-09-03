using System;
using System.Runtime.Caching;

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
                throw new ArgumentNullException("cache");
            }
            this.cache = cache;
            DefaultCacheDuration = 60*20;
        }

        /// <summary>
        ///     Seconds to cache objects for by default
        /// </summary>
        public int DefaultCacheDuration { get; set; }

        private DateTimeOffset DefaultExpiryDateTime
        {
            get
            {
                return DateTimeOffset.Now.AddSeconds(DefaultCacheDuration);
            }
        }

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
                throw new ArgumentNullException("item");
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

            if (item is Lazy<T>)
            {
                return ((Lazy<T>) item).Value;
            }

            return default(T);
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory)
        {
            return GetOrAdd(key, addItemFactory, DefaultExpiryDateTime);
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, DateTimeOffset expires)
        {
            return GetOrAdd(key, addItemFactory, new CacheItemPolicy {AbsoluteExpiration = expires});
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, TimeSpan slidingExpiration)
        {
            return GetOrAdd(key, addItemFactory, new CacheItemPolicy {SlidingExpiration = slidingExpiration});
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


        public ObjectCache ObjectCache => cache;

        private static void EnsureRemovedCallbackDoesNotReturnTheLazy<T>(CacheItemPolicy policy)
        {
            if (policy != null && policy.RemovedCallback != null)
            {
                var originallCallback = policy.RemovedCallback;
                policy.RemovedCallback = (args) =>
                {
                    //unwrap the cache item in a callback given one is specified
                    if (args != null && args.CacheItem != null)
                    {
                        var item = args.CacheItem.Value;
                        if (item is Lazy<T>)
                        {
                            var lazyCacheItem = (Lazy<T>) item;
                            args.CacheItem.Value = lazyCacheItem.IsValueCreated ? lazyCacheItem.Value : default(T);
                        }
                    }
                    originallCallback(args);
                };
            }
        }


        public void Remove(string key)
        {
            ValidateKey(key);
            cache.Remove(key);
        }

        private void ValidateKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentOutOfRangeException("key", @"Cache keys cannot be empty or whitespace");
            }
        }
    }
}