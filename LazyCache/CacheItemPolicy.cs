using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache
{
    [Obsolete(
        "CacheItemPolicy was part of System.Runtime.Caching which is no longer used by LazyCache. " +
        "This class is a fake used to maintain backward compatibility and will be removed in a later version." +
        "Change to MemoryCacheEntryOptions instead")]
    public class CacheItemPolicy : MemoryCacheEntryOptions
    {
        public PostEvictionCallbackRegistration RemovedCallback => PostEvictionCallbacks.FirstOrDefault();
    }
}