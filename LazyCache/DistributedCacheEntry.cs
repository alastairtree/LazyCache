using LazyCache;
using Microsoft.Extensions.Caching.Distributed;

public sealed class DistributedCacheEntry
{
    public string Key { get; internal set; }

    public object Value { get; internal set; }

    public DistributedCacheEntryOptions DistributedCacheEntryOptions { get; private set; }
    

    public void SetOptions(DistributedCacheEntryOptions options)
    {
        DistributedCacheEntryOptions = options;
    }

    public DistributedCacheEntry(string key, object value, DistributedCacheEntryOptions distributedCacheEntryOptions) : this(key, distributedCacheEntryOptions)
    {
        Value = value;
    }

    public DistributedCacheEntry(string key, DistributedCacheEntryOptions distributedCacheEntryOptions) : this(key)
    {
        DistributedCacheEntryOptions = distributedCacheEntryOptions;
    }

    public DistributedCacheEntry(string key)
    {
        Key = key;
        DistributedCacheEntryOptions = new DistributedCacheDefaults().BuildOptions();
    }

    public void SetValue(object value)
    {
        Value = value;
    }
}