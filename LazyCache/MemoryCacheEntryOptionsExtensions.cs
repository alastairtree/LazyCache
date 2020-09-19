using System;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache
{
    public class LazyCacheEntryOptions : MemoryCacheEntryOptions
    {
        public ExpirationMode ExpirationMode { get; set; }
        public TimeSpan ImmediateAbsoluteExpirationRelativeToNow { get; set; }

        public static LazyCacheEntryOptions WithImmediateAbsoluteExpiration(DateTimeOffset absoluteExpiration)
        {
            var delay = absoluteExpiration.Subtract(DateTimeOffset.UtcNow);
            return new LazyCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration,
                ExpirationMode = ExpirationMode.ImmediateExpiration,
                ImmediateAbsoluteExpirationRelativeToNow = delay
            };
        }

        public static LazyCacheEntryOptions WithImmediateAbsoluteExpiration(TimeSpan absoluteExpiration)
        {
            return new LazyCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration,
                ExpirationMode = ExpirationMode.ImmediateExpiration,
                ImmediateAbsoluteExpirationRelativeToNow = absoluteExpiration
            };
        }
    }

    public static class LazyCacheEntryOptionsExtension {
        public static LazyCacheEntryOptions SetAbsoluteExpiration(this LazyCacheEntryOptions option, DateTimeOffset absoluteExpiration,
            ExpirationMode mode)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));

            var delay = absoluteExpiration.Subtract(DateTimeOffset.UtcNow);
            option.AbsoluteExpiration = absoluteExpiration;
            option.ExpirationMode = mode;
            option.ImmediateAbsoluteExpirationRelativeToNow = delay;
            return option;
        }

        public static LazyCacheEntryOptions SetAbsoluteExpiration(this LazyCacheEntryOptions option, TimeSpan absoluteExpiration,
            ExpirationMode mode)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));

            option.AbsoluteExpirationRelativeToNow = absoluteExpiration;
            option.ExpirationMode = mode;
            option.ImmediateAbsoluteExpirationRelativeToNow = absoluteExpiration;
            return option;
        }
    }
}