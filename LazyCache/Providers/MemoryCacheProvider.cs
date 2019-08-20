using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache.Providers
{
    public class MemoryCacheProvider : ICacheProvider
    {
        internal readonly IMemoryCache cache;
        private List<string> _keys;

        public MemoryCacheProvider(IMemoryCache cache)
        {
            this.cache = cache;
            _keys = new List<string>();
        }

        public void Set(string key, object item, MemoryCacheEntryOptions policy)
        {
            _keys.Add(key);
            cache.Set(key, item, policy);
        }

        public object Get(string key)
        {
            var attemptedResult = cache.TryGetValue(key, out var result);

            //Remove the key from the key cache if it exists and this didn't return a result
            if (attemptedResult == false && _keys.Contains(key))
                _keys.Remove(key);

            return result;
        }

        public object GetOrCreate<T>(string key, Func<ICacheEntry, T> factory)
        {
            if (!_keys.Contains(key))
                _keys.Add(key);

            return cache.GetOrCreate(key, factory);
        }

        public void Remove(string key)
        {
            _keys.Remove(key);
            cache.Remove(key);
        }

        public void Remove(Func<IEnumerable<string>, IEnumerable<string>> keyPredicate)
        {
            //Search the keys for any matches to the predicate
            var keyMatches = keyPredicate(_keys).ToList();

            //Remove each of the matches
            foreach (var match in keyMatches) 
                Remove(match);
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<ICacheEntry, Task<T>> factory)
        {
            if (!_keys.Contains(key))
                _keys.Add(key);

            return cache.GetOrCreateAsync(key, factory);
        }

        public void Dispose()
        {
            _keys = null;
            cache?.Dispose();
        }
    }
}