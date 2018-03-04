using System;
using System.Threading;
using System.Threading.Tasks;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache
{
    public class CachingService : IAppCache
    {
        private readonly Lazy<ICacheProvider> cacheProvider;

        private readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);

        public CachingService() : this(DefaultCacheProvider)
        {
        }

        public CachingService(Lazy<ICacheProvider> cacheProvider)
        {
            this.cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        }


        public CachingService(Func<ICacheProvider> cacheProviderFactory)
        {
            if (cacheProviderFactory == null) throw new ArgumentNullException(nameof(cacheProviderFactory));
            cacheProvider = new Lazy<ICacheProvider>(cacheProviderFactory);
        }

        public CachingService(ICacheProvider cache) : this(() => cache)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
        }

        public static Lazy<ICacheProvider> DefaultCacheProvider { get; set; } 
            = new Lazy<ICacheProvider>(() => new MemoryCacheProvider());

        /// <summary>
        /// Seconds to cache objects for by default
        /// </summary>
        public virtual int DefaultCacheDurationSeconds { get; set; } = 60 * 20;

        /// <summary>
        /// Seconds to cache objects for by default
        /// </summary>
        [Obsolete("DefaultCacheDuration has been replaced with DefaultCacheDurationSeconds")]

        public virtual int DefaultCacheDuration
        {
            get => DefaultCacheDurationSeconds;
            set => DefaultCacheDurationSeconds = value;
        }

        private DateTimeOffset DefaultExpiryDateTime => DateTimeOffset.Now.AddSeconds(DefaultCacheDurationSeconds);

        public virtual void Add<T>(string key, T item)
        {
            Add(key, item, DefaultExpiryDateTime);
        }

        public virtual void Add<T>(string key, T item, DateTimeOffset expires)
        {
            Add(key, item, new MemoryCacheEntryOptions {AbsoluteExpiration = expires});
        }

        public virtual void Add<T>(string key, T item, TimeSpan slidingExpiration)
        {
            Add(key, item, new MemoryCacheEntryOptions {SlidingExpiration = slidingExpiration});
        }

        public virtual void Add<T>(string key, T item, MemoryCacheEntryOptions policy)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            ValidateKey(key);

            CacheProvider.Set(key, item, policy);
        }

        public virtual T Get<T>(string key)
        {
            ValidateKey(key);

            var item = CacheProvider.Get(key);

            return UnwrapLazy<T>(item);
        }

        public virtual Task<T> GetAsync<T>(string key)
        {
            ValidateKey(key);

            var item = CacheProvider.Get(key);

            return UnwrapAsyncLazys<T>(item);
        }

        public virtual T GetOrAdd<T>(string key, Func<T> addItemFactory)
        {
            return GetOrAdd(key, addItemFactory, DefaultExpiryDateTime);
        }

        public virtual async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory,
            MemoryCacheEntryOptions policy)
        {
            ValidateKey(key);

            EnsureEvictionCallbackDoesNotReturnTheAsyncLazy<T>(policy);

            //any other way of doing this?
            //var cacheItem = CacheProvider.GetOrCreateAsync(key, async entry =>
            //    await new AsyncLazy<T>(async () => {
            //        entry.SetOptions(policy);
            //        return await addItemFactory.Invoke();
            //    }
            //).Value);

            //var cacheItem = CacheProvider.GetOrCreateAsync<T>(key, async entry =>
            //{
            //    entry.SetOptions(policy);
            //    return new AsyncLazy<T>(async () => await addItemFactory.Invoke());
            //});

            object cacheItem;
            await locker.WaitAsync().ConfigureAwait(false); //TODO: do we really need to lock - or can we lock just the key?
            try
            {
                cacheItem = CacheProvider.GetOrCreate(key, entry =>
                    {
                        entry.SetOptions(policy);
                        var value = new AsyncLazy<T>(addItemFactory);
                        return (object) value;
                    }
                );
            }
            finally
            {
                locker.Release();
            }

            try
            {
                var result = UnwrapAsyncLazys<T>(cacheItem);

                if (result.IsCanceled || result.IsFaulted)
                    CacheProvider.Remove(key);

                return await result.ConfigureAwait(false);
            }
            catch //addItemFactory errored so do not cache the exception
            {
                CacheProvider.Remove(key);
                throw;
            }
        }

        public virtual T GetOrAdd<T>(string key, Func<T> addItemFactory, DateTimeOffset expires)
        {
            return GetOrAdd(key, addItemFactory, new MemoryCacheEntryOptions {AbsoluteExpiration = expires});
        }

        public virtual T GetOrAdd<T>(string key, Func<T> addItemFactory, TimeSpan slidingExpiration)
        {
            return GetOrAdd(key, addItemFactory, new MemoryCacheEntryOptions {SlidingExpiration = slidingExpiration});
        }

        public virtual T GetOrAdd<T>(string key, Func<T> addItemFactory, MemoryCacheEntryOptions policy)
        {
            ValidateKey(key);

            EnsureEvictionCallbackDoesNotReturnTheLazy<T>(policy);

            object cacheItem;
            locker.Wait(); //TODO: do we really need this?
            try
            {
                cacheItem = CacheProvider.GetOrCreate(key, entry =>
                    {
                        entry.SetOptions(policy);
                        var value = new Lazy<T>(addItemFactory);
                        return (object) value;
                    }
                );
            }
            finally
            {
                locker.Release();
            }

            try
            {
                return UnwrapLazy<T>(cacheItem);
            }
            catch //addItemFactory errored so do not cache the exception
            {
                CacheProvider.Remove(key);
                throw;
            }
        }

        public virtual void Remove(string key)
        {
            ValidateKey(key);
            CacheProvider.Remove(key);
        }

        public virtual ICacheProvider CacheProvider => cacheProvider.Value;

        public virtual Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory)
        {
            return GetOrAddAsync(key, addItemFactory, DefaultExpiryDateTime);
        }

        public virtual Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, DateTimeOffset expires)
        {
            return GetOrAddAsync(key, addItemFactory, new MemoryCacheEntryOptions {AbsoluteExpiration = expires});
        }

        public virtual Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory,
            TimeSpan slidingExpiration)
        {
            return GetOrAddAsync(key, addItemFactory,
                new MemoryCacheEntryOptions {SlidingExpiration = slidingExpiration});
        }

        protected virtual T UnwrapLazy<T>(object item)
        {
            if (item is Lazy<T> lazy)
                return lazy.Value;

            if (item is T variable)
                return variable;

            if (item is AsyncLazy<T> asyncLazy)
                return asyncLazy.Value.ConfigureAwait(false).GetAwaiter().GetResult();

            if (item is Task<T> task)
                return task.Result;

            return default(T);
        }

        protected virtual Task<T> UnwrapAsyncLazys<T>(object item)
        {
            if (item is AsyncLazy<T> asyncLazy)
                return asyncLazy.Value;

            if (item is Task<T> task)
                return task;

            if (item is Lazy<T> lazy)
                return Task.FromResult(lazy.Value);

            if (item is T variable)
                return Task.FromResult(variable);

            return Task.FromResult(default(T));
        }

        protected virtual void EnsureEvictionCallbackDoesNotReturnTheLazy<T>(MemoryCacheEntryOptions policy)
        {
            if (policy?.PostEvictionCallbacks != null)
                foreach (var item in policy.PostEvictionCallbacks)
                {
                    var originallCallback = item.EvictionCallback;
                    item.EvictionCallback = (key, value, reason, state) =>
                    {
                        //unwrap the cache item in a callback given one is specified
                        if (value is Lazy<T> cacheItem)
                            value = cacheItem.IsValueCreated ? cacheItem.Value : default(T);
                        ;
                        originallCallback(key, value, reason, state);
                    };
                }
        }

        protected virtual void EnsureEvictionCallbackDoesNotReturnTheAsyncLazy<T>(MemoryCacheEntryOptions policy)
        {
            if (policy?.PostEvictionCallbacks != null)
                foreach (var item in policy.PostEvictionCallbacks)
                {
                    var originalCallback = item.EvictionCallback;
                    item.EvictionCallback = (key, value, reason, state) =>
                    {
                        //unwrap the cache item in a callback given one is specified
                        if (value is AsyncLazy<T> cacheItem)
                            value = cacheItem.IsValueCreated ? cacheItem.Value : Task.FromResult(default(T));
                        originalCallback(key, value, reason, state);
                    };
                }
        }

        protected virtual void ValidateKey(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentOutOfRangeException(nameof(key), "Cache keys cannot be empty or whitespace");
        }
    }
}