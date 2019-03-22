using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Threading.Tasks;


namespace LazyCache.Providers
{
    public class DistributedCacheProvider : IDistributedCacheProvider
    {
        internal readonly IDistributedCache cache;

        internal readonly JsonSerializerSettings deserializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            TypeNameHandling = TypeNameHandling.All
        };

    public DistributedCacheProvider(IDistributedCache cache)
        {
            this.cache = cache;
        }

        internal object Set(DistributedCacheEntry entry)
        {
            cache.SetString(entry.Key, JsonConvert.SerializeObject(entry.Value, deserializerSettings), entry.DistributedCacheEntryOptions);
            return entry.Value;
        }

        internal async Task SetAsync(DistributedCacheEntry entry)
        {
            await cache.SetStringAsync(entry.Key, JsonConvert.SerializeObject(entry.Value, deserializerSettings), entry.DistributedCacheEntryOptions);
        }

        public void Set(string key, object item, DistributedCacheEntryOptions policy)
        {
            cache.SetString(key, JsonConvert.SerializeObject(item, deserializerSettings), policy);
        }

        private static string ToBson<T>(T value)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BsonDataWriter datawriter = new BsonDataWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(datawriter, value);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private static T FromBson<T>(string base64data)
        {
            byte[] data = Convert.FromBase64String(base64data);

            using (MemoryStream ms = new MemoryStream(data))
            using (BsonDataReader reader = new BsonDataReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }

        public T Get<T>(string key)
        {
            var valueJson = cache.GetString(key);
            if (valueJson == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(valueJson, deserializerSettings);
        }

        public object Get(string key)
        {
            var valueJson = cache.GetString(key);
            if (valueJson == null)
                return null;
            return JsonConvert.DeserializeObject(valueJson, deserializerSettings);
        }

        public object GetOrCreate<T>(string key, Func<DistributedCacheEntry, T> func)
        {
            if (!TryGetValue(key, out T result))
            {
                var entry = new DistributedCacheEntry(key);
                result = func(entry);
                entry.SetValue(result);
                Set(entry);
            }

            return result;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<DistributedCacheEntry, Task<T>> func)
        {
            if (!TryGetValue(key, out T result))
            {
                var entry = new DistributedCacheEntry(key);
                result = func(entry).GetAwaiter().GetResult();
                entry.SetValue(result);

                await SetAsync(entry);
            }

            return result;
        }

        public void Remove(string key)
        {
            cache.Remove(key);
        }

        private bool TryGetValue<T>(string key, out T value)
        {
            value = Get<T>(key);
            return value != null && !value.Equals(default(T));
        }
    }
}