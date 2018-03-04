using System;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache
{
    public class CacheDefaults
    {
        public virtual int DefaultCacheDurationSeconds { get; set; } = 60 * 20;

        internal MemoryCacheEntryOptions BuildOptions()
        {
            return new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(DefaultCacheDurationSeconds)
            };
        }
    }
}