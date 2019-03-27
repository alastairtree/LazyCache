using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache
{
    public static class AppCacheExtensions
    {
        public static void Add<T>(this IAppCache cache, string key, T item)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            cache.Add(key, item, cache.DefaultCachePolicy.BuildOptions());
        }

        public static void Add<T>(this IAppCache cache, string key, T item, DateTimeOffset expires)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            cache.Add(key, item, new MemoryCacheEntryOptions {AbsoluteExpiration = expires});
        }

        public static void Add<T>(this IAppCache cache, string key, T item, TimeSpan slidingExpiration)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            cache.Add(key, item, new MemoryCacheEntryOptions {SlidingExpiration = slidingExpiration});
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

            return cache.GetOrAdd(key, entry =>
            {
                entry.SetOptions(policy);
                return addItemFactory();
            });
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

            return cache.GetOrAddAsync(key, entry =>
            {
                entry.SetOptions(policy);
                return addItemFactory();
            });
        }
    }
}