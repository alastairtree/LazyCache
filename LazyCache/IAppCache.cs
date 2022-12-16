using System;
using System.Threading.Tasks;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache
{
    public interface IAppCache
    {
        /// <summary>
        /// Gets an instance of <see cref="ICacheProvider"/>.
        /// </summary>
        /// <remarks>
        /// Default implementation <see cref="MemoryCacheProvider"/>.
        /// </remarks>
        ICacheProvider CacheProvider { get; }

        /// <summary>
        /// Defines the value for <see cref="MemoryCacheEntryOptions.AbsoluteExpiration"/>.
        /// </summary>
        CacheDefaults DefaultCachePolicy { get; }

        /// <summary>
        /// Tries to add an item to the cache.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="item">Object that will be cached.</param>
        /// <param name="policy">Instance of <see cref="MemoryCacheEntryOptions"/> that can be used to configure behavior of the cached item.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="key"/> is empty or whitespace.
        /// </exception>
        void Add<T>(string key, T item, MemoryCacheEntryOptions policy);

        /// <summary>
        /// Tries to synchronously retrieve an object associated with the provided key.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <returns>A cached object</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="key"/> is empty or whitespace.</exception>
        T Get<T>(string key);

        /// <summary>
        /// Tries to asynchronously retrieve an object associated with the provided key.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>A task with the cached object.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="key"/> is empty or whitespace.
        /// </exception>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Tries to synchronously retrieve the object associated with the provided key if present.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Instance that will capture the cached object value.</param>
        /// <returns>True if the item was found.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="key"/> is empty or whitespace.
        /// </exception>
        bool TryGetValue<T>(string key, out T value);

        /// <summary>
        /// Tries to synchronously retrieve an object associated with the provided key.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="addItemFactory">A method that will be executed to add an item to the cache.</param>
        /// <returns>A cached object or null.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="key"/> is empty or whitespace.
        /// </exception>
        /// <remarks>
        /// Method will bubble up any exception that may be thrown by <paramref name="addItemFactory"/> delegate.
        /// </remarks>
        T GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory);

        /// <summary>
        /// Tries to synchronously retrieve an object associated with the provided key.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="addItemFactory">A method that will be executed to add an item to the cache.</param>
        /// <param name="policy">Instance of <see cref="MemoryCacheEntryOptions"/> that can be used to configure behavior of the cached item.</param>
        /// <returns>A cached object or null.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="key"/> is empty or whitespace.
        /// </exception>
        /// <remarks>
        /// Method will bubble up any exception that may be thrown by <paramref name="addItemFactory"/> delegate.
        /// </remarks>
        T GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory, MemoryCacheEntryOptions policy);

        /// <summary>
        /// Tries to asynchronously retrieve an object associated with the provided key.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="addItemFactory">A method that will be executed to add an item to the cache.</param>
        /// <returns>A task with the cached object.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="key"/> is empty or whitespace.
        /// </exception>
        /// <remarks>
        /// Method will bubble up any exception that may be thrown by <paramref name="addItemFactory"/> delegate.
        /// </remarks>
        Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory);

        /// <summary>
        /// Tries to asynchronously retrieve an object associated with the provided key.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="addItemFactory">A method that will be executed to add an item to the cache.</param>
        /// <param name="policy">Instance of <see cref="MemoryCacheEntryOptions"/> that can be used to configure behavior of the cached item.</param>
        /// <returns>A task with the cached object.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="key"/> is empty or whitespace.
        /// </exception>
        Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory, MemoryCacheEntryOptions policy);

        /// <summary>
        /// Tries to remove the object associated with the provided key.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="key"/> is empty or whitespace.
        /// </exception>
        void Remove(string key);
    }
}