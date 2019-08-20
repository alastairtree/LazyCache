using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache
{
    public interface ICacheProvider : IDisposable
    {
        void Set(string key, object item, MemoryCacheEntryOptions policy);
        object Get(string key);
        object GetOrCreate<T>(string key, Func<ICacheEntry, T> func);
        void Remove(string key);
        void Remove(Func<IEnumerable<string>, IEnumerable<string>> keyPredicate);
        Task<T> GetOrCreateAsync<T>(string key, Func<ICacheEntry, Task<T>> func);
    }
}