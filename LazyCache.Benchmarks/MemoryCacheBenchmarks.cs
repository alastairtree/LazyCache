using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class MemoryCacheBenchmarks
    { 
        public const string CacheKey = nameof(CacheKey);

        public IMemoryCache MemCache;
        public IAppCache AppCache;
        public ComplexObject ComplexObject;

        [GlobalSetup]
        public void Setup()
        {
            MemCache = new MemoryCache(new MemoryCacheOptions());
            AppCache = new CachingService();

            ComplexObject = new ComplexObject();
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
        public CachingService LazyCache_Init() => new CachingService();

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
         * Benchmark Get Methods
         *
         */

        [Benchmark(Baseline = true), BenchmarkCategory(nameof(IAppCache.Get))]
        public ComplexObject DotNetMemoryCache_Get() => MemCache.Get<ComplexObject>(CacheKey);

        [Benchmark, BenchmarkCategory(nameof(IAppCache.Get))]
        public ComplexObject LazyCache_Get() => AppCache.Get<ComplexObject>(CacheKey);

        /*
         *
         * Benchmark GetOrAdd Methods
         *
         */

        [Benchmark(Baseline = true), BenchmarkCategory(nameof(IAppCache.GetOrAdd))]
        public ComplexObject DotNetMemoryCache_GetOrAdd() => MemCache.GetOrCreate(CacheKey, entry => ComplexObject);

        [Benchmark, BenchmarkCategory(nameof(IAppCache.GetOrAdd))]
        public ComplexObject LazyCache_GetOrAdd() => AppCache.GetOrAdd(CacheKey, entry => ComplexObject);

        /*
         *
         * Benchmark GetOrAddAsync Methods
         *
         */

        
        [Benchmark(Baseline = true), BenchmarkCategory(nameof(IAppCache.GetOrAddAsync))]
        public async Task<ComplexObject> DotNetMemoryCache_GetOrAddAsync() => await MemCache.GetOrCreateAsync(CacheKey, async entry => await Task.FromResult(ComplexObject));

        [Benchmark, BenchmarkCategory(nameof(IAppCache.GetOrAddAsync))]
        public async Task<ComplexObject> LazyCache_GetOrAddAsync() => await AppCache.GetOrAddAsync(CacheKey, async entry => await Task.FromResult(ComplexObject));
    }
}