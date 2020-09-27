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
|                               Method |       Mean |       Error |   StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
|------------------------------------- |-----------:|------------:|---------:|------:|--------:|-------:|-------:|-------:|----------:|
|               DotNetMemoryCache_Init | 1,814.2 ns | 1,080.95 ns | 59.25 ns |  1.00 |    0.00 | 0.1850 | 0.0916 | 0.0019 |    1560 B |
|                       LazyCache_Init | 3,265.5 ns |   599.75 ns | 32.87 ns |  1.80 |    0.07 | 0.3090 | 0.1526 |      - |    2600 B |
|                                      |            |             |          |       |         |        |        |        |           |
|                DotNetMemoryCache_Set |   504.1 ns |    42.38 ns |  2.32 ns |  1.00 |    0.00 | 0.0496 |      - |      - |     416 B |
|                        LazyCache_Set |   841.6 ns |   172.51 ns |  9.46 ns |  1.67 |    0.02 | 0.0801 |      - |      - |     672 B |
|                                      |            |             |          |       |         |        |        |        |           |
|           DotNetMemoryCache_Get_Miss |   201.1 ns |     3.54 ns |  0.19 ns |  1.00 |    0.00 |      - |      - |      - |         - |
|                   LazyCache_Get_Miss |   241.1 ns |    13.94 ns |  0.76 ns |  1.20 |    0.00 |      - |      - |      - |         - |
|                                      |            |             |          |       |         |        |        |        |           |
|            DotNetMemoryCache_Get_Hit |   242.2 ns |    28.93 ns |  1.59 ns |  1.00 |    0.00 |      - |      - |      - |         - |
|                    LazyCache_Get_Hit |   280.4 ns |    10.45 ns |  0.57 ns |  1.16 |    0.01 |      - |      - |      - |         - |
|                                      |            |             |          |       |         |        |        |        |           |
|      DotNetMemoryCache_GetOrAdd_Miss |   269.9 ns |     6.57 ns |  0.36 ns |  1.00 |    0.00 | 0.0076 |      - |      - |      64 B |
|              LazyCache_GetOrAdd_Miss |   368.5 ns |    60.35 ns |  3.31 ns |  1.37 |    0.01 | 0.0191 |      - |      - |     160 B |
|                                      |            |             |          |       |         |        |        |        |           |
|       DotNetMemoryCache_GetOrAdd_Hit |   269.1 ns |     4.48 ns |  0.25 ns |  1.00 |    0.00 | 0.0076 |      - |      - |      64 B |
|               LazyCache_GetOrAdd_Hit |   377.1 ns |    10.57 ns |  0.58 ns |  1.40 |    0.00 | 0.0191 |      - |      - |     160 B |
|                                      |            |             |          |       |         |        |        |        |           |
| DotNetMemoryCache_GetOrAddAsync_Miss |   312.7 ns |    53.05 ns |  2.91 ns |  1.00 |    0.00 | 0.0162 |      - |      - |     136 B |
|         LazyCache_GetOrAddAsync_Miss |   507.5 ns |    33.96 ns |  1.86 ns |  1.62 |    0.02 | 0.0362 |      - |      - |     304 B |
|                                      |            |             |          |       |         |        |        |        |           |
|  DotNetMemoryCache_GetOrAddAsync_Hit |   314.5 ns |    65.34 ns |  3.58 ns |  1.00 |    0.00 | 0.0162 |      - |      - |     136 B |
|          LazyCache_GetOrAddAsync_Hit |   535.9 ns |    47.83 ns |  2.62 ns |  1.70 |    0.03 | 0.0448 |      - |      - |     376 B |

|                                                 Method |             Mean |           Error |          StdDev |  Gen 0 |  Gen 1 |  Gen 2 |  Allocated |
|------------------------------------------------------- |-----------------:|----------------:|----------------:|-------:|-------:|-------:|-----------:|
|                                              Init_CRUD |       5,115.1 ns |        991.0 ns |        54.32 ns | 0.4730 | 0.2365 | 0.0076 |     3.9 KB |
| Several_initializations_of_1Mb_object_with_200ms_delay | 207,329,988.9 ns | 31,342,899.9 ns | 1,718,010.11 ns |      - |      - |      - | 1031.75 KB |