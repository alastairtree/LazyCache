using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

public interface IDistributedCacheProvider
{
    object Get(string key);
    T Get<T>(string key);
    void Set(string key, object item, DistributedCacheEntryOptions policy);
    object GetOrCreate<T>(string key, Func<DistributedCacheEntry, T> func);
    Task<T> GetOrCreateAsync<T>(string key, Func<DistributedCacheEntry, Task<T>> func);
    void Remove(string key);
}