using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class MemoryCacheBenchmarks
    { 
        public const string CacheKey = nameof(CacheKey);

        public IMemoryCache MemCache;
        public IMemoryCache PopulatedMemCache;
        public IAppCache AppCache;
        public IAppCache PopulatedAppCache;
        public ComplexObject ComplexObject;

        [GlobalSetup]
        public void Setup()
        {
            ComplexObject = new ComplexObject();

            MemCache = new MemoryCache(new MemoryCacheOptions());
            PopulatedMemCache = new MemoryCache(new MemoryCacheOptions());

            AppCache = new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())));
            PopulatedAppCache = new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())));

            PopulatedAppCache.Add(CacheKey, ComplexObject);
            PopulatedMemCache.Set(CacheKey, ComplexObject);
        }

        [GlobalCleanup]
        public void Cleanup() => MemCache.Dispose();

        /*
         *
         * Benchmark Cache Initialization
         *
         */

        [Benchmark(Baseline = true), BenchmarkCategory("Init")]
        public MemoryCache DotNetMemoryCache_Init() => new MemoryCache(new MemoryCacheOptions());

        [Benchmark, BenchmarkCategory("Init")]
        public CachingService LazyCache_Init() => new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())));

        /*
         *
         * Benchmark Add Methods
         *
         */

        [Benchmark(Baseline = true), BenchmarkCategory(nameof(IAppCache.Add))]
        public void DotNetMemoryCache_Set() => MemCache.Set(CacheKey, ComplexObject);

        [Benchmark, BenchmarkCategory(nameof(IAppCache.Add))]
        public void LazyCache_Set() => AppCache.Add(CacheKey, ComplexObject);

        /*
         *
         * Benchmark Get Methods With a Cache Miss
         *
         */

        [Benchmark(Baseline = true), BenchmarkCategory(nameof(IAppCache.Get) + "_Miss")]
        public ComplexObject DotNetMemoryCache_Get_Miss() => MemCache.Get<ComplexObject>(CacheKey);

        [Benchmark, BenchmarkCategory(nameof(IAppCache.Get) + "_Miss")]
        public ComplexObject LazyCache_Get_Miss() => AppCache.Get<ComplexObject>(CacheKey);

        /*
         *
         * Benchmark Get Methods With a Cache Hit
         *
         */

        [Benchmark(Baseline = true), BenchmarkCategory(nameof(IAppCache.Get) + "_Hit")]
        public ComplexObject DotNetMemoryCache_Get_Hit() => PopulatedMemCache.Get<ComplexObject>(CacheKey);

        [Benchmark, BenchmarkCategory(nameof(IAppCache.Get) + "_Hit")]
        public ComplexObject LazyCache_Get_Hit() => PopulatedAppCache.Get<ComplexObject>(CacheKey);

        /*
         *
         * Benchmark GetOrAdd Methods With Cache Miss
         *
         */

        [Benchmark(Baseline = true), BenchmarkCategory(nameof(IAppCache.GetOrAdd) + "_Miss")]
        public ComplexObject DotNetMemoryCache_GetOrAdd_Miss() => MemCache.GetOrCreate(CacheKey, entry => ComplexObject);

        [Benchmark, BenchmarkCategory(nameof(IAppCache.GetOrAdd) + "_Miss")]
        public ComplexObject LazyCache_GetOrAdd_Miss() => AppCache.GetOrAdd(CacheKey, entry => ComplexObject);

        /*
         *
         * Benchmark GetOrAdd Methods With Cache Hit
         *
         */

        [Benchmark(Baseline = true), BenchmarkCategory(nameof(IAppCache.GetOrAdd) + "_Hit")]
        public ComplexObject DotNetMemoryCache_GetOrAdd_Hit() => PopulatedMemCache.GetOrCreate(CacheKey, entry => ComplexObject);

        [Benchmark, BenchmarkCategory(nameof(IAppCache.GetOrAdd) + "_Hit")]
        public ComplexObject LazyCache_GetOrAdd_Hit() => PopulatedAppCache.GetOrAdd(CacheKey, entry => ComplexObject);

        /*
         *
         * Benchmark GetOrAddAsync Methods With Cache Miss
         *
         */


        [Benchmark(Baseline = true), BenchmarkCategory(nameof(IAppCache.GetOrAddAsync) + "_Miss")]
        public Task<ComplexObject> DotNetMemoryCache_GetOrAddAsync_Miss() => MemCache.GetOrCreateAsync(CacheKey, entry => Task.FromResult(ComplexObject));

        [Benchmark, BenchmarkCategory(nameof(IAppCache.GetOrAddAsync) + "_Miss")]
        public Task<ComplexObject> LazyCache_GetOrAddAsync_Miss() => AppCache.GetOrAddAsync(CacheKey, entry => Task.FromResult(ComplexObject));

        /*
         *
         * Benchmark GetOrAddAsync Methods With Cache Hit
         *
         */

        [Benchmark(Baseline = true), BenchmarkCategory(nameof(IAppCache.GetOrAddAsync) + "_Hit")]
        public Task<ComplexObject> DotNetMemoryCache_GetOrAddAsync_Hit() => PopulatedMemCache.GetOrCreateAsync(CacheKey, entry => Task.FromResult(ComplexObject));

        [Benchmark, BenchmarkCategory(nameof(IAppCache.GetOrAddAsync) + "_Hit")]
        public Task<ComplexObject> LazyCache_GetOrAddAsync_Hit() => PopulatedAppCache.GetOrAddAsync(CacheKey, entry => Task.FromResult(ComplexObject));
    }
}
