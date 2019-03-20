using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
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
            = new Lazy<ICacheProvider>(() => 
                new MemoryCacheProvider(
                    new MemoryCache(
                        new MemoryCacheOptions())
                ));

        /// <summary>
        ///     Seconds to cache objects for by default
        /// </summary>
        [Obsolete("DefaultCacheDuration has been replaced with DefaultCacheDurationSeconds")]
        public virtual int DefaultCacheDuration
        {
            get => DefaultCachePolicy.DefaultCacheDurationSeconds;
            set => DefaultCachePolicy.DefaultCacheDurationSeconds = value;
        }

        /// <summary>
        ///     Policy defining how long items should be cached for unless specified
        /// </summary>
        public virtual CacheDefaults DefaultCachePolicy { get; set; } = new CacheDefaults();

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

            return GetValueFromLazy<T>(item);
        }

        public virtual Task<T> GetAsync<T>(string key)
        {
            ValidateKey(key);

            var item = CacheProvider.Get(key);

            return GetValueFromAsyncLazy<T>(item);
        }

        public virtual T GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory)
        {
            ValidateKey(key);

            object cacheItem;
            locker.Wait(); //TODO: do we really need this? Could we just lock on the key?
            try
            {
                cacheItem = CacheProvider.GetOrCreate<object>(key, entry =>
                    new Lazy<T>(() =>
                    {
                        var result = addItemFactory(entry);
                        EnsureEvictionCallbackDoesNotReturnTheAsyncOrLazy<T>(entry.PostEvictionCallbacks);
                        return result;
                    })
                );
            }
            finally
            {
                locker.Release();
            }

            try
            {
                return GetValueFromLazy<T>(cacheItem);
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

        public virtual async Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory)
        {
            ValidateKey(key);

            object cacheItem;

            // Ensure only one thread can place an item into the cache provider at a time.
            // We are not evaluating the addItemFactory inside here - that happens outside the lock,
            // below, and guarded using the async lazy. Here we just ensure only one thread can place 
            // the AsyncLazy into the cache at one time

            await locker.WaitAsync()
                .ConfigureAwait(
                    false); //TODO: do we really need to lock everything here - faster if we could lock on just the key?
            try
            {
                cacheItem = CacheProvider.GetOrCreate<object>(key, entry =>
                    new AsyncLazy<T>(() =>
                    {
                        var result = addItemFactory(entry);
                        EnsureEvictionCallbackDoesNotReturnTheAsyncOrLazy<T>(entry.PostEvictionCallbacks);
                        return result;
                    })
                );
            }
            finally
            {
                locker.Release();
            }

            try
            {
                var result = GetValueFromAsyncLazy<T>(cacheItem);

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

        protected virtual T GetValueFromLazy<T>(object item)
        {
            switch (item)
            {
                case Lazy<T> lazy:
                    return lazy.Value;
                case T variable:
                    return variable;
                case AsyncLazy<T> asyncLazy:
                    // this is async to sync - and should not really happen as long as GetOrAddAsync is used for an async
                    // value. Only happens when you cache something async and then try and grab it again later using
                    // the non async methods.
                    return asyncLazy.Value.ConfigureAwait(false).GetAwaiter().GetResult();
                case Task<T> task:
                    return task.Result;
            }

            return default(T);
        }

        protected virtual Task<T> GetValueFromAsyncLazy<T>(object item)
        {
            switch (item)
            {
                case AsyncLazy<T> asyncLazy:
                    return asyncLazy.Value;
                case Task<T> task:
                    return task;
                // this is sync to async and only happens if you cache something sync and then get it later async
                case Lazy<T> lazy:
                    return Task.FromResult(lazy.Value);
                case T variable:
                    return Task.FromResult(variable);
            }

            return Task.FromResult(default(T));
        }

        protected virtual void EnsureEvictionCallbackDoesNotReturnTheAsyncOrLazy<T>(
            IList<PostEvictionCallbackRegistration> callbackRegistrations)
        {
            if (callbackRegistrations != null)
                foreach (var item in callbackRegistrations)
                {
                    var originalCallback = item.EvictionCallback;
                    item.EvictionCallback = (key, value, reason, state) =>
                    {
                        // before the original callback we need to unwrap the Lazy that holds the cache item
                        if (value is AsyncLazy<T> asyncCacheItem)
                            value = asyncCacheItem.IsValueCreated ? asyncCacheItem.Value : Task.FromResult(default(T));
                        else if (value is Lazy<T> cacheItem)
                            value = cacheItem.IsValueCreated ? cacheItem.Value : default(T);

                        // pass the unwrapped cached value to the original callback
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