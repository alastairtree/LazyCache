using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.Caching.Memory;

namespace LazyCache.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class MemoryCacheBenchmarksRealLifeScenarios
    {
        public const string CacheKey = nameof(CacheKey);
        
        [Benchmark]
        public void Init_CRUD()
        {

        }

        [Benchmark]
        public void Init_many_writes_with_many_evictions()
        {

        }

        [Benchmark]
        public void Init_single_long_write_with_multiple_concurrent_read_writes()
        {

        }
    }
}