using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace LazyCache.Benchmarks
{
    public class BenchmarkConfig: ManualConfig
    {
        public BenchmarkConfig()
            => AddJob(Job.ShortRun)
              .AddDiagnoser(MemoryDiagnoser.Default)
              .AddLogger(new ConsoleLogger())
              .AddColumn(TargetMethodColumn.Method)
              .AddAnalyser(EnvironmentAnalyser.Default);
    }
}