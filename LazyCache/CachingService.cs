using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache
{
    public class CachingService : IAppCache
    {
        public CachingService() : this(new MemoryCache(new MemoryCacheOptions()))
        {
        }

        public CachingService(IMemoryCache cache)
        {
            MemoryCache = cache ?? throw new ArgumentNullException(nameof(cache));
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
            Add(key, item, new MemoryCacheEntryOptions { AbsoluteExpiration = expires});
        }

        public void Add<T>(string key, T item, TimeSpan slidingExpiration)
        {
            Add(key, item, new MemoryCacheEntryOptions {SlidingExpiration = slidingExpiration});
        }

        public void Add<T>(string key, T item, MemoryCacheEntryOptions policy)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            ValidateKey(key);

            MemoryCache.Set(key, item, policy);
        }

        public T Get<T>(string key)
        {
            ValidateKey(key);

            var item = MemoryCache.Get(key);

            return UnwrapLazy<T>(item);
        }


        public async Task<T> GetAsync<T>(string key)
        {
            ValidateKey(key);

            var item = MemoryCache.Get(key);

            return await UnwrapAsyncLazys<T>(item);
        }


        public T GetOrAdd<T>(string key, Func<T> addItemFactory)
        {
            return GetOrAdd(key, addItemFactory, DefaultExpiryDateTime);
        }


        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, MemoryCacheEntryOptions policy)
        {
            ValidateKey(key);

            EnsureRemovedCallbackDoesNotReturnTheAsyncLazy<T>(policy);

            var cacheItem = MemoryCache.GetOrCreate(key, entry =>
                {
                    entry.SetOptions(policy);
                    var value = new AsyncLazy<T>(addItemFactory);
                    return (object) value;
                }
            );

            try
            {
                var result = UnwrapAsyncLazys<T>(cacheItem);

                if (result.IsCanceled || result.IsFaulted)
                    MemoryCache.Remove(key);

                return await result;
            }
            catch //addItemFactory errored so do not cache the exception
            {
                MemoryCache.Remove(key);
                throw;
            }
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, DateTimeOffset expires)
        {
            return GetOrAdd(key, addItemFactory, new MemoryCacheEntryOptions {AbsoluteExpiration = expires});
        }


        public T GetOrAdd<T>(string key, Func<T> addItemFactory, TimeSpan slidingExpiration)
        {
            return GetOrAdd(key, addItemFactory, new MemoryCacheEntryOptions {SlidingExpiration = slidingExpiration});
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, MemoryCacheEntryOptions policy)
        {
            ValidateKey(key);

            EnsureRemovedCallbackDoesNotReturnTheLazy<T>(policy);

            var cacheItem = MemoryCache.GetOrCreate(key, entry =>
                {
                    entry.SetOptions(policy);
                    var value = new Lazy<T>(addItemFactory);
                    return (object)value;
                }
            );

            try
            {
                return UnwrapLazy<T>(cacheItem);
            }
            catch //addItemFactory errored so do not cache the exception
            {
                MemoryCache.Remove(key);
                throw;
            }
        }


        public void Remove(string key)
        {
            ValidateKey(key);
            MemoryCache.Remove(key);
        }

        public IMemoryCache MemoryCache { get; }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory)
        {
            return await GetOrAddAsync(key, addItemFactory, DefaultExpiryDateTime);
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, DateTimeOffset expires)
        {
            return await GetOrAddAsync(key, addItemFactory, new MemoryCacheEntryOptions {AbsoluteExpiration = expires});
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, TimeSpan slidingExpiration)
        {
            return await GetOrAddAsync(key, addItemFactory, new MemoryCacheEntryOptions {SlidingExpiration = slidingExpiration});
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
                return asyncLazy.Value.ConfigureAwait(false).GetAwaiter().GetResult();

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

        private static void EnsureRemovedCallbackDoesNotReturnTheLazy<T>(MemoryCacheEntryOptions policy)
        {
            if (policy?.PostEvictionCallbacks != null)
            {
                foreach (var item in policy.PostEvictionCallbacks)
                {
                    var originallCallback = item.EvictionCallback;
                    item.EvictionCallback = (key, value, reason, state) =>
                    {
                        //unwrap the cache item in a callback given one is specified
                        if (value is Lazy<T> cacheItem)
                            value = cacheItem.IsValueCreated ? cacheItem.Value : default(T); ;
                        originallCallback(key, value, reason, state);
                    };
                }
            }
        }

        private static void EnsureRemovedCallbackDoesNotReturnTheAsyncLazy<T>(MemoryCacheEntryOptions policy)
        {
            if (policy?.PostEvictionCallbacks != null)
                foreach (var item in policy.PostEvictionCallbacks)
                {
                    var originalCallback = item.EvictionCallback;
                    item.EvictionCallback = (key, value, reason, state) =>
                    {
                        //unwrap the cache item in a callback given one is specified
                        if (value is AsyncLazy<T> cacheItem)
                        {
                            value = cacheItem.IsValueCreated ? cacheItem.Value : Task.FromResult(default(T));
                        }
                        originalCallback(key, value, reason, state);
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