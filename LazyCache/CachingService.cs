using System;
using System.Runtime.Caching;

namespace LazyCache
{
    using System.Diagnostics;

    public class CacheChangedEventArgs : EventArgs
    {
        public string Key { get; private set; }
        public ChangeType Change { get; private set; }

        public enum ChangeType
        {
            Added,
            Removed,
            Retrieved
        }

        public CacheChangedEventArgs(string key, ChangeType changeType)
        {
            Key = key;
            Change = changeType;
        }
    }

    public class CachingService : IAppCache
    {
        public event EventHandler<CacheChangedEventArgs> ItemAddedEvent;
        public event EventHandler<CacheChangedEventArgs> ItemRemovedEvent;
        public event EventHandler<CacheChangedEventArgs> ItemRetrievedEvent;

        private void RaiseItemAddedEvent(string key)
        {
            var handler = ItemAddedEvent;
            if (handler != null)
            {
                handler(this, new CacheChangedEventArgs(key, CacheChangedEventArgs.ChangeType.Added));
            }
        }

        private void RaiseItemRemovedEvent(string key)
        {
            var handler = ItemRemovedEvent;
            if (handler != null)
            {
                handler(this, new CacheChangedEventArgs(key, CacheChangedEventArgs.ChangeType.Removed));
            }
        }

        private void RaiseItemRetrievedEvent(string key)
        {
            var handler = ItemRetrievedEvent;
            if (handler != null)
            {
                handler(this, new CacheChangedEventArgs(key, CacheChangedEventArgs.ChangeType.Retrieved));
            }
        }

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

            AttachRemovedCallbackHandler(policy);

            cache.Set(key, item, policy);

            RaiseItemAddedEvent(key);
        }

        private void AttachRemovedCallbackHandler(CacheItemPolicy policy)
        {
            if (policy != null)
            {
                policy.RemovedCallback += args => this.RaiseItemRemovedEvent(args.CacheItem.Key);
            }
        }

        public T Get<T>(string key)
        {
            ValidateKey(key);
             
            var item = cache[key];

            if (item is T)
            {
                RaiseItemRetrievedEvent(key);
                return (T) item;
            }

            if (item is Lazy<T>)
            {
                RaiseItemRetrievedEvent(key);
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
            AttachRemovedCallbackHandler(policy);

            var existingCacheItem = cache.AddOrGetExisting(key, newLazyCacheItem, policy);

            if (existingCacheItem != null)
            {
                if (existingCacheItem is T)
                {
                    var eci = (T) existingCacheItem;
                    RaiseItemRetrievedEvent(key);
                    return eci;
                }

                if (existingCacheItem is Lazy<T>)
                {
                    var eci = ((Lazy<T>) existingCacheItem).Value;
                    RaiseItemRetrievedEvent(key);
                    return eci;
                }

                return default(T);
            }

            try
            {
                var val = newLazyCacheItem.Value;

                RaiseItemAddedEvent(key);

                return val;
            }
            catch //addItemFactory errored so do not cache the exception
            {
                cache.Remove(key);
                throw;
            }
        }

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

            RaiseItemRemovedEvent(key);
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