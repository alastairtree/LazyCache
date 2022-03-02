<img align="right" src="https://raw.githubusercontent.com/alastairtree/LazyCache/master/artwork/logo-256.png?raw=true" />

# Lazy Cache #

[![Build status](https://ci.appveyor.com/api/projects/status/oca98pp4safs4vj2/branch/master?svg=true)](https://ci.appveyor.com/project/alastairtree/lazycache/branch/master)
![AppVeyor tests](https://img.shields.io/appveyor/tests/alastairtree/lazycache.svg)
[![NuGet](https://img.shields.io/nuget/v/LazyCache.svg)](https://www.nuget.org/packages/LazyCache/)
![Nuget](https://img.shields.io/nuget/dt/LazyCache.svg)


Lazy cache is a simple in-memory caching service. It has a developer friendly 
generics based API, and provides a thread safe cache implementation that 
guarantees to only execute your cachable delegates once (it's lazy!). Under 
the hood it leverages Microsoft.Extensions.Caching and Lazy<T> to provide performance and 
reliability in heavy load scenarios.

## Download ##

LazyCache is available using [nuget](https://www.nuget.org/packages/LazyCache/). To install LazyCache, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console)

```Powershell
PM> Install-Package LazyCache
```

## Quick start

See the [quick start wiki](https://github.com/alastairtree/LazyCache/wiki/Quickstart)

## Sample code

```csharp
// Create our cache service using the defaults (Dependency injection ready).
// By default it uses a single shared cache under the hood so cache is shared out of the box (but you can configure this)
IAppCache cache = new CachingService();

// Declare (but don't execute) a func/delegate whose result we want to cache
Func<ComplexObjects> complexObjectFactory = () => methodThatTakesTimeOrResources();

// Get our ComplexObjects from the cache, or build them in the factory func 
// and cache the results for next time under the given key
ComplexObjects cachedResults = cache.GetOrAdd("uniqueKey", complexObjectFactory);
```

As you can see the magic happens in the `GetOrAdd()` method which gives the consumer an atomic and tidy way to add caching to your code. It leverages a factory delegate `Func` and generics to make it easy to add cached method calls to your app. 

It means you avoid the usual "Check the cache - execute the factory function - add results to the cache" pattern, saves you writing the double locking cache pattern and means you can be a lazy developer!

## What should I use it for?

LazyCache suits the caching of database calls, complex object graph building routines and web service calls that should be cached for performance. 
Allows items to be cached for long or short periods, but defaults to 20 mins.

## .Net framework and dotnet core support?

The latest version targets netstandard 2.0. See [.net standard implementation support](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support)

For dotnet core 2, .net framwork net461 or above, netstandard 2+, use LazyCache 2 or above.

For .net framework without netstandard 2 support such as net45 net451 net46 use LazyCache 0.7 - 1.x

For .net framework 4.0 use LazyCache 0.6


## Features ##

- Simple API with familiar sliding or absolute expiration
- Guaranteed single evaluation of your factory delegate whose results you want to cache
- Strongly typed generics based API. No need to cast your cached objects every time you retrieve them
- Stops you inadvertently caching an exception by removing Lazys that evaluate to an exception
- Thread safe, concurrency ready
- Async compatible - lazy single evaluation of async delegates using `GetOrAddAsync()`
- Interface based API and built in `MockCache` to support test driven development and dependency injection
- Leverages a provider model on top of IMemoryCache under the hood and can be extended with your own implementation
- Good test coverage

## Documentation

* [The wiki](https://github.com/alastairtree/LazyCache/wiki)
* [Adding caching to a .net application and make it faster](https://alastaircrabtree.com/the-easy-way-to-add-caching-to-net-application-and-make-it-faster-is-called-lazycache/)

## Sample Application

See [CacheDatabaseQueriesApiSample](/CacheDatabaseQueriesApiSample) for an example of how to use LazyCache to cache the results of an Entity framework query in
a web api controller. Watch how the cache saves trips to the database and results are returned to the client far quicker from the 
in-memory cache

## Contributing

If you have an idea or want to fix an issue please open an issue on Github to discuss it and it will be considered. 

If you have code to share you should submit a pull request: fork the repo, then create a branch on that repo with your changes, when you are happy create a pull Request from your branch into LazyCache master for review. See https://help.github.com/en/articles/creating-a-pull-request-from-a-fork. 

LazyCache is narrow in focus and well established so unlikely to accept massive changes out of nowhere but come talk about on GitHub and we can all collaborate on something that works for everyone. It is also quite extensible so you may be able to extend it in your project or add a companion library if necessary.


