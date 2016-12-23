using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace LazyCache
{
    public class CachingService : IAppCache
    {
        public CachingService() : this(MemoryCache.Default)
        {
        }

        public CachingService(ObjectCache cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            ObjectCache = cache;
            DefaultCacheDuration = 60*20;
        }

        /// <summary>
        /// Seconds to cache objects for by default
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
                throw new ArgumentNullException(nameof(item));
            ValidateKey(key);

            ObjectCache.Set(key, item, policy);
        }

        public T Get<T>(string key)
        {
            ValidateKey(key);

            var item = ObjectCache[key];

            return UnwrapLazy<T>(item);
        }


        public async Task<T> GetAsync<T>(string key)
        {
            ValidateKey(key);

            var item = ObjectCache[key];

            return await UnwrapAsyncLazys<T>(item);
        }


        public T GetOrAdd<T>(string key, Func<T> addItemFactory)
        {
            return GetOrAdd(key, addItemFactory, DefaultExpiryDateTime);
        }


        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, CacheItemPolicy policy)
        {
            ValidateKey(key);

            var newLazyCacheItem = new AsyncLazy<T>(addItemFactory);

            EnsureRemovedCallbackDoesNotReturnTheAsyncLazy<T>(policy);

            var existingCacheItem = ObjectCache.AddOrGetExisting(key, newLazyCacheItem, policy);

            if (existingCacheItem != null)
                return await UnwrapAsyncLazys<T>(existingCacheItem);

            try
            {
                var result = newLazyCacheItem.Value;

                if (result.IsCanceled || result.IsFaulted)
                    ObjectCache.Remove(key);

                return await result;
            }
            catch //addItemFactory errored so do not cache the exception
            {
                ObjectCache.Remove(key);
                throw;
            }
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

            var existingCacheItem = ObjectCache.AddOrGetExisting(key, newLazyCacheItem, policy);

            if (existingCacheItem != null)
                return UnwrapLazy<T>(existingCacheItem);

            try
            {
                return newLazyCacheItem.Value;
            }
            catch //addItemFactory errored so do not cache the exception
            {
                ObjectCache.Remove(key);
                throw;
            }
        }


        public void Remove(string key)
        {
            ValidateKey(key);
            ObjectCache.Remove(key);
        }

        public ObjectCache ObjectCache { get; }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory)
        {
            return await GetOrAddAsync(key, addItemFactory, DefaultExpiryDateTime);
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, DateTimeOffset expires)
        {
            return await GetOrAddAsync(key, addItemFactory, new CacheItemPolicy {AbsoluteExpiration = expires});
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, TimeSpan slidingExpiration)
        {
            return await GetOrAddAsync(key, addItemFactory, new CacheItemPolicy {SlidingExpiration = slidingExpiration});
        }

        private static T UnwrapLazy<T>(object item)
        {
            var lazy = item as Lazy<T>;
            if (lazy != null)
                return lazy.Value;

            if (item is T)
                return (T) item;

            var asyncLazy = item as AsyncLazy<T>;
            if (asyncLazy != null)
                return asyncLazy.Value.Result;

            var task = item as Task<T>;
            if (task != null)
                return task.Result;

            return default(T);
        }

        private static async Task<T> UnwrapAsyncLazys<T>(object item)
        {
            var asyncLazy = item as AsyncLazy<T>;
            if (asyncLazy != null)
                return await asyncLazy.Value;

            var task = item as Task<T>;
            if (task != null)
                return await task;

            var lazy = item as Lazy<T>;
            if (lazy != null)
                return lazy.Value;

            if (item is T)
                return (T) item;

            return default(T);
        }

        private static void EnsureRemovedCallbackDoesNotReturnTheLazy<T>(CacheItemPolicy policy)
        {
            if (policy?.RemovedCallback != null)
            {
                var originallCallback = policy.RemovedCallback;
                policy.RemovedCallback = args =>
                {
                    //unwrap the cache item in a callback given one is specified
                    var item = args?.CacheItem?.Value as Lazy<T>;
                    if (item != null)
                        args.CacheItem.Value = item.IsValueCreated ? item.Value : default(T);
                    originallCallback(args);
                };
            }
        }

        private static void EnsureRemovedCallbackDoesNotReturnTheAsyncLazy<T>(CacheItemPolicy policy)
        {
            if (policy?.RemovedCallback != null)
            {
                var originallCallback = policy.RemovedCallback;
                policy.RemovedCallback = args =>
                {
                    //unwrap the cache item in a callback given one is specified
                    var item = args?.CacheItem?.Value as AsyncLazy<T>;
                    if (item != null)
                        args.CacheItem.Value = item.IsValueCreated ? item.Value : Task.FromResult(default(T));
                    originallCallback(args);
                };
            }
        }

        private void ValidateKey(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentOutOfRangeException(nameof(key), "Cache keys cannot be empty or whitespace");
        }
    }
}