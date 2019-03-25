using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LazyCache
{
    public class HybridCachingService : IDistributedAppCache
    {
        private readonly Lazy<IDistributedCacheProvider> cacheProvider;

        private readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);

        public HybridCachingService(Lazy<IDistributedCacheProvider> cacheProvider)
        {
            this.cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        }

        public HybridCachingService(Func<IDistributedCacheProvider> cacheProviderFactory)
        {
            if (cacheProviderFactory == null) throw new ArgumentNullException(nameof(cacheProviderFactory));
            cacheProvider = new Lazy<IDistributedCacheProvider>(cacheProviderFactory);
        }

        public HybridCachingService(IDistributedCacheProvider cache) : this(() => cache)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        ///     Seconds to cache objects for by default
        /// </summary>
        [Obsolete("DefaultCacheDuration has been replaced with DefaultCacheDurationSeconds")]
        public virtual int DefaultCacheDuration
        {
            get => DefaultCachePolicy.DefaultCacheDurationSeconds;
            set => DefaultCachePolicy.DefaultCacheDurationSeconds = value;
        }

        public virtual IDistributedCacheProvider DistributedCacheProvider => cacheProvider.Value;

        /// <summary>
        ///     Policy defining how long items should be cached for unless specified
        /// </summary>
        public virtual DistributedCacheDefaults DefaultCachePolicy { get; set; } = new DistributedCacheDefaults();

        public virtual void Add<T>(string key, T item, DistributedCacheEntryOptions policy)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            ValidateKey(key);

            DistributedCacheProvider.Set(key, item, policy);
        }

        public virtual T Get<T>(string key)
        {
            ValidateKey(key);

            var item = DistributedCacheProvider.Get<T>(key);

            return GetValueFromLazy<T>(item);
        }

        public virtual Task<T> GetAsync<T>(string key)
        {
            ValidateKey(key);

            var item = DistributedCacheProvider.Get(key);

            return GetValueFromAsyncLazy<T>(item);
        }

        public virtual T GetOrAdd<T>(string key, Func<DistributedCacheEntry, T> addItemFactory)
        {
            ValidateKey(key);
            DistributedCacheEntry temporaryCacheEntry = null;
            object cacheItem;
            locker.Wait(); //TODO: do we really need this? Could we just lock on the key?

            try
            {
                cacheItem = DistributedCacheProvider.GetOrCreate<object>(key, entry =>
                    new Lazy<T>(() =>
                    {
                        temporaryCacheEntry = entry;
                        var result = addItemFactory(entry);
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
                var toBeCached = GetValueFromLazy<T>(cacheItem);
                DistributedCacheProvider.Set(key, toBeCached, temporaryCacheEntry != null ? temporaryCacheEntry.DistributedCacheEntryOptions : DefaultCachePolicy.BuildOptions());
                return toBeCached;
            }
            catch //addItemFactory errored so do not cache the exception
            {
                DistributedCacheProvider.Remove(key);
                throw;
            }
        }

        public virtual void Remove(string key)
        {
            ValidateKey(key);
            DistributedCacheProvider.Remove(key);
        }


        public virtual async Task<T> GetOrAddAsync<T>(string key, Func<DistributedCacheEntry, Task<T>> addItemFactory)
        {
            ValidateKey(key);

            object cacheItem;
            DistributedCacheEntry temporaryCacheEntry = null;
            // Ensure only one thread can place an item into the cache provider at a time.
            // We are not evaluating the addItemFactory inside here - that happens outside the lock,
            // below, and guarded using the async lazy. Here we just ensure only one thread can place 
            // the AsyncLazy into the cache at one time

            await locker.WaitAsync()
                .ConfigureAwait(
                    false); //TODO: do we really need to lock everything here - faster if we could lock on just the key?
            try
            {
                // var value = await DistributedCacheProvider.GetOrCreateAsync(key, addItemFactory);
                // cacheItem = new Lazy<T>(() => (T) value);

                cacheItem = DistributedCacheProvider.GetOrCreate<object>(key, entry =>
                    new AsyncLazy<T>(() =>
                    {
                        temporaryCacheEntry = entry;
                        var result = addItemFactory(entry);
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
                    DistributedCacheProvider.Remove(key);

                var toBeCached = await result.ConfigureAwait(false);
                DistributedCacheProvider.Set(key, toBeCached, temporaryCacheEntry != null ? temporaryCacheEntry.DistributedCacheEntryOptions : DefaultCachePolicy.BuildOptions());
                return toBeCached;
            }
            catch //addItemFactory errored so do not cache the exception
            {
                DistributedCacheProvider.Remove(key);
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

        protected virtual void ValidateKey(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentOutOfRangeException(nameof(key), "Cache keys cannot be empty or whitespace");
        }
    }
}