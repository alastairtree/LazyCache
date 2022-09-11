using System;

namespace LazyCache
{
    public class LazyCacheOptions
    {
        public int DefaultCacheDurationSeconds { get; set; } = 60 * 20;
        public int NumberOfKeyLocks { get; set; } = Math.Max(Environment.ProcessorCount * 8, 32);

    }
}