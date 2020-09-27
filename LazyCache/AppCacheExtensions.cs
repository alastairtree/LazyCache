using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace LazyCache
{
    public static class AppCacheExtensions
    {
        [Obsolete("This method has been deprecated. Use Set<T> instead.", false)]
        public static void Add<T>(this IAppCache cache, string key, T item)
        {
            Set(cache, key, item);
        }

        [Obsolete("This method has been deprecated. Use Set<T> instead.", false)]
        public static void Add<T>(this IAppCache cache, string key, T item, DateTimeOffset expires)
        {
            Set(cache, key, item, expires);
        }

        [Obsolete("This method has been deprecated. Use Set<T> instead.", false)]
        public static void Add<T>(this IAppCache cache, string key, T item, TimeSpan slidingExpiration)
        {
            Set(cache, key, item, slidingExpiration);
        }

        public static T GetOrAdd<T>(this IAppCache cache, string key, Func<T> addItemFactory)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            return cache.GetOrAdd(key, addItemFactory, cache.DefaultCachePolicy.BuildOptions());
        }

        public static T GetOrAdd<T>(this IAppCache cache, string key, Func<T> addItemFactory, DateTimeOffset expires)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            return cache.GetOrAdd(key, addItemFactory, new MemoryCacheEntryOptions {AbsoluteExpiration = expires});
        }

        public static T GetOrAdd<T>(this IAppCache cache, string key, Func<T> addItemFactory, DateTimeOffset expires, ExpirationMode mode)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            switch (mode)
            {
                case ExpirationMode.LazyExpiration:
                    return cache.GetOrAdd(key, addItemFactory, new MemoryCacheEntryOptions { AbsoluteExpiration = expires });
                default:
                    return cache.GetOrAdd(key, addItemFactory, new LazyCacheEntryOptions().SetAbsoluteExpiration(expires, mode));
            }
        }

        public static T GetOrAdd<T>(this IAppCache cache, string key, Func<T> addItemFactory,
            TimeSpan slidingExpiration)
        {
            return cache.GetOrAdd(key, addItemFactory,
                new MemoryCacheEntryOptions {SlidingExpiration = slidingExpiration});
        }

        public static T GetOrAdd<T>(this IAppCache cache, string key, Func<T> addItemFactory,
            MemoryCacheEntryOptions policy)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            return cache.GetOrAdd(key, _=> addItemFactory(), policy);
        }

        public static Task<T> GetOrAddAsync<T>(this IAppCache cache, string key, Func<Task<T>> addItemFactory)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            return cache.GetOrAddAsync(key, addItemFactory, cache.DefaultCachePolicy.BuildOptions());
        }

        public static Task<T> GetOrAddAsync<T>(this IAppCache cache, string key, Func<Task<T>> addItemFactory,
            DateTimeOffset expires)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            return cache.GetOrAddAsync(key, addItemFactory, new MemoryCacheEntryOptions {AbsoluteExpiration = expires});
        }

        public static Task<T> GetOrAddAsync<T>(this IAppCache cache, string key, Func<Task<T>> addItemFactory,
            DateTimeOffset expires, ExpirationMode mode)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            switch (mode)
            {
                case ExpirationMode.LazyExpiration:
                    return cache.GetOrAddAsync(key, addItemFactory, new MemoryCacheEntryOptions { AbsoluteExpiration = expires });
                default:
                    return cache.GetOrAddAsync(key, addItemFactory, new LazyCacheEntryOptions().SetAbsoluteExpiration(expires, mode));
            }
        }

        public static Task<T> GetOrAddAsync<T>(this IAppCache cache, string key, Func<Task<T>> addItemFactory,
            TimeSpan slidingExpiration)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            return cache.GetOrAddAsync(key, addItemFactory,
                new MemoryCacheEntryOptions {SlidingExpiration = slidingExpiration});
        }

        public static Task<T> GetOrAddAsync<T>(this IAppCache cache, string key, Func<Task<T>> addItemFactory,
            MemoryCacheEntryOptions policy)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            return cache.GetOrAddAsync(key, _=> addItemFactory(), policy);
        }

        public static void Set<T>(this IAppCache cache, string key, T item)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            cache.Set(key, item, cache.DefaultCachePolicy.BuildOptions());
        }

        public static void Set<T>(this IAppCache cache, string key, T item, DateTimeOffset expires)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            cache.Set(key, item, new MemoryCacheEntryOptions { AbsoluteExpiration = expires });
        }

        public static void Set<T>(this IAppCache cache, string key, T item, TimeSpan slidingExpiration)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            cache.Set(key, item, new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration });
        }
    }
}