using System;
using Microsoft.Extensions.Caching.Distributed;

namespace LazyCache
{
    public class DistributedCacheDefaults
    {
        public virtual int DefaultCacheDurationSeconds { get; set; } = 60 * 20;

        internal DistributedCacheEntryOptions BuildOptions()
        {
            return new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(DefaultCacheDurationSeconds),
                SlidingExpiration = TimeSpan.FromSeconds(200)
            };
        }
    }
}