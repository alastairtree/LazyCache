using System;

namespace LazyCache
{
    public enum ExpirationMode
    {
        /// <summary>
        ///     This is the default for Memory cache - expired items are removed from the cache
        ///     the next time that key is accessed. This is the most performant, and so the default,
        ///     because no timers are required to removed expired items, but it does mean that
        ///     PostEvictionCallbacks may fire later than expected, or not at all.
        /// </summary>
        LazyExpiration,

        /// <summary>
        ///     Use a timer to force eviction of expired items from the cache as soon as they expire.
        ///     This will then trigger PostEvictionCallbacks at the expected time. This uses more resources
        ///     than LazyExpiration.
        /// </summary>
        [Obsolete("Use ExpirationMode.ImmediateEviction instead - this name is miss-leading")]
        ImmediateExpiration,

        /// <summary>
        ///     Use a timer to force eviction of expired items from the cache as soon as they expire.
        ///     This will then trigger PostEvictionCallbacks at the expected time. This uses more resources
        ///     than LazyExpiration.
        /// </summary>
        ImmediateEviction
    }
}