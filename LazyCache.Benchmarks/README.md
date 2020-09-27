# LazyCache.Benchmarks
This project is dedicated towards benchmarking (using [BenchmarkDotNet](https://benchmarkdotnet.org/index.html)) the basic functionality of LazyCache such that contributors and maintainers can verify the efficacy of changes towards the project - for better or for worse.

## Note to readers
While it is always a good idea to understand performance of your third party libraries, it is rare that you will be concerned with performance on the scale of nanoseconds such that this library operates on. Be wary of premature optimization.

# How to run
- Ensure you have the requisite dotnet SDKs found in _LazyCache.Benchmarks.csproj_
- Clone the project
- Open your favorite terminal, navigate to the Benchmark Project
- `dotnet run -c Release`
- Pick your desired benchmark suite via numeric entry

If you are interested in benchmarking a specific method (after making changes to it, for instance), you can conveniently filter down to one specific benchmark, e.g. `dotnet run -c Release -- -f *Get` will only run the benchmarks for `IAppCache.Get` implementations, likewise with `*GetOrAddAsync`, or other methods.

# Contributing
If you have ideas for one or more benchmarks not covered here, please add an issue describing what you would like to see. Pull requests are always welcome!

# Benchmark Types
There are two types of benchmarks available.

## Basics
The basic benchmarks are small and laser-focused on testing individual aspects of LazyCache. This suite of benchmarks uses the out-of-the-box MemoryCache from dotnet [seen here](https://github.com/dotnet/runtime/blob/master/src/libraries/Microsoft.Extensions.Caching.Memory/src/) as a baseline, to demonstrate the "cost" of LazyCache in comparison.

## Integration
These benchmarks are designed to showcase full use-cases of LazyCache by chaining together various operations. As an example, with the Memory Diagnoser from BenchmarkDotNet, we can verify that concurrent calls to initialize a cache item correctly spin up one instance of said item, with the subsequent calls awaiting its result.

### Gotchas
Remember that BenchmarkDotNet dutifully monitors allocations inside the benchmark method, and _only_ the method. At the time of writing, the default instance of the MemoryCacheProvider is static, and allocations into this cache will **not** be monitored by BenchmarkDotNet. For all benchmarks, please ensure you are creating new instances of the Service, Provider, and backing Cache.

# Benchmarks

```
// * Summary *
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.1082 (1903/May2019Update/19H1)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]   : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  ShortRun : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1
WarmupCount=3
```
|                          Method |       Mean |     Error |   StdDev | Ratio |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|-------------------------------- |-----------:|----------:|---------:|------:|-------:|-------:|------:|----------:|
|          DotNetMemoryCache_Init | 1,779.9 ns | 643.50 ns | 35.27 ns |  1.00 | 0.1831 | 0.0916 |     - |    1560 B |
|                  LazyCache_Init |   447.4 ns |  14.25 ns |  0.78 ns |  0.25 | 0.1020 | 0.0005 |     - |     856 B |
|                                 |            |           |          |       |        |        |       |           |
|           DotNetMemoryCache_Set |   486.9 ns |   3.15 ns |  0.17 ns |  1.00 | 0.0496 |      - |     - |     416 B |
|                   LazyCache_Set |   788.6 ns |  45.65 ns |  2.50 ns |  1.62 | 0.0801 |      - |     - |     672 B |
|                                 |            |           |          |       |        |        |       |           |
|           DotNetMemoryCache_Get |   198.6 ns |   3.97 ns |  0.22 ns |  1.00 |      - |      - |     - |         - |
|                   LazyCache_Get |   231.2 ns |   2.64 ns |  0.14 ns |  1.16 |      - |      - |     - |         - |
|                                 |            |           |          |       |        |        |       |           |
|      DotNetMemoryCache_GetOrAdd |   260.8 ns |   9.54 ns |  0.52 ns |  1.00 | 0.0076 |      - |     - |      64 B |
|              LazyCache_GetOrAdd |   356.8 ns |  23.08 ns |  1.27 ns |  1.37 | 0.0191 |      - |     - |     160 B |
|                                 |            |           |          |       |        |        |       |           |
| DotNetMemoryCache_GetOrAddAsync |   371.8 ns |  13.73 ns |  0.75 ns |  1.00 | 0.0334 |      - |     - |     280 B |
|         LazyCache_GetOrAddAsync |   552.5 ns |  40.98 ns |  2.25 ns |  1.49 | 0.0534 |      - |     - |     448 B |

|                                                 Method |             Mean |           Error |          StdDev |  Gen 0 |  Gen 1 |  Gen 2 |  Allocated |
|------------------------------------------------------- |-----------------:|----------------:|----------------:|-------:|-------:|-------:|-----------:|
|                                              Init_CRUD |       5,115.1 ns |        991.0 ns |        54.32 ns | 0.4730 | 0.2365 | 0.0076 |     3.9 KB |
| Several_initializations_of_1Mb_object_with_200ms_delay | 207,329,988.9 ns | 31,342,899.9 ns | 1,718,010.11 ns |      - |      - |      - | 1031.75 KB |