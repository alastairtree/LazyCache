using System;
using System.Management;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class MemoryCacheBenchmarksRealLifeScenarios
    {
        public const string CacheKey = nameof(CacheKey);

        public ComplexObject ComplexObject1;
        public ComplexObject ComplexObject2;
        public ComplexObject ComplexObject3;
        public ComplexObject ComplexObject4;
        public ComplexObject ComplexObject5;

        // Trying not to introduce artificial allocations below - just measuring what the library itself needs
        [GlobalSetup]
        public void Setup()
        {
            ComplexObject1 = new ComplexObject();
            ComplexObject2 = new ComplexObject();
            ComplexObject3 = new ComplexObject();
            ComplexObject4 = new ComplexObject();
            ComplexObject5 = new ComplexObject();
        }

        [Benchmark]
        public ComplexObject Init_CRUD()
        {
            var cache = new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions()))) as IAppCache;

            cache.Add(CacheKey, ComplexObject1);

            var obj = cache.Get<ComplexObject>(CacheKey);

            obj.Int = 256;
            cache.Add(CacheKey, obj);

            cache.Remove(CacheKey);

            return obj;
        }

        // Benchmark memory usage to ensure only a single instance of the object is created
        // Due to the nature of AsyncLazy, this test should also only take the the time it takes to create
        //   one instance  of the object.
        [Benchmark]
        public async Task<byte[]> Several_initializations_of_1Mb_object_with_200ms_delay()
        {
            var cache = new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions()))) as IAppCache;

            Task AddByteArrayToCache() =>
                cache.GetOrAddAsync(CacheKey, async () =>
                {
                    await Task.Delay(200);
                    return await Task.FromResult(new byte[1024 * 1024]); // 1Mb
                });

            // Even though the second and third init attempts are later, this whole operation should still take the time of the first
            var creationTask1 = AddByteArrayToCache(); // initialization attempt, or 200ms
            var creationTask2 = Task.Delay(50).ContinueWith(async t => await AddByteArrayToCache());
            var creationTask3 = Task.Delay(150).ContinueWith(async t => await AddByteArrayToCache());

            await Task.WhenAll(creationTask1, creationTask2, creationTask3);
            //await AddByteArrayToCache();
            return cache.Get<byte[]>(CacheKey);
        }
    }
}