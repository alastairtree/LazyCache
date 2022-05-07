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

        private readonly int[] keyLocks;

        public CachingService() : this(DefaultCacheProvider)
        {
        }

        public CachingService(Lazy<ICacheProvider> cacheProvider)
        {
            this.cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
            var lockCount = Math.Max(Environment.ProcessorCount * 8, 32);
            keyLocks = new int[lockCount];
        }

        public CachingService(Func<ICacheProvider> cacheProviderFactory)
        {
            if (cacheProviderFactory == null) throw new ArgumentNullException(nameof(cacheProviderFactory));
            cacheProvider = new Lazy<ICacheProvider>(cacheProviderFactory);
            var lockCount = Math.Max(Environment.ProcessorCount * 8, 32);
            keyLocks = new int[lockCount];

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

            return GetValueFromLazy<T>(item, out _);
        }

        public virtual Task<T> GetAsync<T>(string key)
        {
            ValidateKey(key);

            var item = CacheProvider.Get(key);

            return GetValueFromAsyncLazy<T>(item, out _);
        }

        public virtual bool TryGetValue<T>(string key, out T value)
        {
            ValidateKey(key);

            try
            {
                var contains = CacheProvider.TryGetValue<Lazy<T>>(key, out var lazyFactory);
                value = GetValueFromLazy<T>(lazyFactory, out var _);
                return contains;
            }
            catch (InvalidCastException)
            {
                return CacheProvider.TryGetValue<T>(key, out value);
            }
        }

        public virtual T GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory)
        {
            return GetOrAdd(key, addItemFactory, null);
        }

        public virtual T GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory, MemoryCacheEntryOptions policy)
        {
            ValidateKey(key);

            object cacheItem;

            object CacheFactory(ICacheEntry entry) =>
                new Lazy<T>(() =>
                {
                    var result = addItemFactory(entry);
                    SetAbsoluteExpirationFromRelative(entry);
                    EnsureEvictionCallbackDoesNotReturnTheAsyncOrLazy<T>(entry.PostEvictionCallbacks);
                    return result;
                });

            // acquire lock per key
            uint hash = (uint)key.GetHashCode() % (uint)keyLocks.Length;
            while (Interlocked.CompareExchange(ref keyLocks[hash], 1, 0) == 1) { Thread.Yield(); }

            try
            {
                cacheItem = CacheProvider.GetOrCreate<object>(key, policy, CacheFactory);
            }
            finally
            {
                keyLocks[hash] = 0;
            }

            try
            {
                var result = GetValueFromLazy<T>(cacheItem, out var valueHasChangedType);

                // if we get a cache hit but for something with the wrong type we need to evict it, start again and cache the new item instead
                if (valueHasChangedType)
                {
                    CacheProvider.Remove(key);

                    // acquire lock again
                    hash = (uint)key.GetHashCode() % (uint)keyLocks.Length;
                    while (Interlocked.CompareExchange(ref keyLocks[hash], 1, 0) == 1) { Thread.Yield(); }

                    try
                    {
                        cacheItem = CacheProvider.GetOrCreate<object>(key, CacheFactory);
                    }
                    finally
                    {
                        keyLocks[hash] = 0;
                    }
                    result = GetValueFromLazy<T>(cacheItem, out _ /* we just evicted so type change cannot happen this time */);
                }

                return result;
            }
            catch //addItemFactory errored so do not cache the exception
            {
                CacheProvider.Remove(key);
                throw;
            }
        }

        private static void SetAbsoluteExpirationFromRelative(ICacheEntry entry)
        {
            if (!entry.AbsoluteExpirationRelativeToNow.HasValue) return;

            var absoluteExpiration = DateTimeOffset.UtcNow + entry.AbsoluteExpirationRelativeToNow.Value;
            if (!entry.AbsoluteExpiration.HasValue || absoluteExpiration < entry.AbsoluteExpiration)
                entry.AbsoluteExpiration = absoluteExpiration;
        }

        public virtual void Remove(string key)
        {
            ValidateKey(key);
            CacheProvider.Remove(key);
        }

        public virtual ICacheProvider CacheProvider => cacheProvider.Value;

        public virtual Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory)
        {
            return GetOrAddAsync(key, addItemFactory, null);
        }

        public virtual async Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory,
            MemoryCacheEntryOptions policy)
        {
            ValidateKey(key);

            object cacheItem;

            // Ensure only one thread can place an item into the cache provider at a time.
            // We are not evaluating the addItemFactory inside here - that happens outside the lock,
            // below, and guarded using the async lazy. Here we just ensure only one thread can place 
            // the AsyncLazy into the cache at one time

            // acquire lock
            uint hash = (uint)key.GetHashCode() % (uint)keyLocks.Length;
            while (Interlocked.CompareExchange(ref keyLocks[hash], 1, 0) == 1) { Thread.Yield(); }

            object CacheFactory(ICacheEntry entry) =>
                new AsyncLazy<T>(async () =>
                {
                    var result = await addItemFactory(entry).ConfigureAwait(false);
                    SetAbsoluteExpirationFromRelative(entry);
                    EnsureEvictionCallbackDoesNotReturnTheAsyncOrLazy<T>(entry.PostEvictionCallbacks);
                    return result;
                });

            try
            {
                cacheItem = CacheProvider.GetOrCreate<object>(key, policy, CacheFactory);
            }
            finally
            {
                keyLocks[hash] = 0;
            }

            try
            {
                var result = GetValueFromAsyncLazy<T>(cacheItem, out var valueHasChangedType);

                // if we get a cache hit but for something with the wrong type we need to evict it, start again and cache the new item instead
                if (valueHasChangedType)
                {
                    CacheProvider.Remove(key);

                    // acquire lock
                    hash = (uint)key.GetHashCode() % (uint)keyLocks.Length;
                    while (Interlocked.CompareExchange(ref keyLocks[hash], 1, 0) == 1) { Thread.Yield(); }

                    try
                    {
                        cacheItem = CacheProvider.GetOrCreate<object>(key, CacheFactory);
                    }
                    finally
                    {
                        keyLocks[hash] = 0;
                    }
                    result = GetValueFromAsyncLazy<T>(cacheItem, out _ /* we just evicted so type change cannot happen this time */);
                }

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

        protected virtual T GetValueFromLazy<T>(object item, out bool valueHasChangedType)
        {
            valueHasChangedType = false;
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

            // if they have cached something else with the same key we need to tell caller to reset the cached item
            // although this is probably not the fastest this should not get called on the main use case
            // where you just hit the first switch case above. 
            var itemsType = item?.GetType();
            if (itemsType != null && itemsType.IsGenericType && itemsType.GetGenericTypeDefinition() == typeof(Lazy<>))
            {
                valueHasChangedType = true;
            }

            return default(T);
        }

        protected virtual Task<T> GetValueFromAsyncLazy<T>(object item, out bool valueHasChangedType)
        {
            valueHasChangedType = false;
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

            // if they have cached something else with the same key we need to tell caller to reset the cached item
            // although this is probably not the fastest this should not get called on the main use case
            // where you just hit the first switch case above. 
            var itemsType = item?.GetType();
            if (itemsType != null && itemsType.IsGenericType && itemsType.GetGenericTypeDefinition() == typeof(AsyncLazy<>))
            {
                valueHasChangedType = true;
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