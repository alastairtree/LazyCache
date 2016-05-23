# Lazy Cache #

Lazy cache is a simple in-memory caching service. It has a developer friendly 
generics based API, and provides a thread safe cache implementation that 
guarantees to only execute your cachable delegates once (it's lazy!). Under 
the hood it leverages ObjectCache and Lazy<T> to provide performance and 
reliability in heavy load scenarios.

## Download ##

LazyCache is available using [nuget](https://www.nuget.org/packages/LazyCache/). To install LazyCache, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console)


    PM> Install-Package LazyCache

## Sample code ##

    // Create our cache service using the defaults (Dependency injection ready).
    // Uses MemoryCache.Default under the hood so cache is shared out of the box
    IAppCache cache = new CachingService();

    // Declare (but don't execute) a func/delegate whose result we want to cache
    Func<ComplexObjects> complexObjectFactory = () => methodThatTakesTimeOrResources();
    
    // Get our ComplexObjects from the cache, or build them in the factory func 
    // and cache the results for next time under the given key
    ComplexObject cachedResults = cache.GetOrAdd("uniqueKey", complexObjectFactory);
    
As you can see the magic happens in the `GetOrAdd()` method which gives the consumer an atomic and tidy way to add caching to your code. It leverages a factory delegate `Func` and generics to make it easy to add cached method calls to your app. 

It means you avoid the usual "Check the cache - execute the factory function - add results to the cache" pattern, saves you writing the double locking cache pattern and means you can be a lazy developer!

## Use case ##

Suits the caching of database calls, complex object graph building routines and web service calls that should be cached for performance. Allows items to be cached for long or short periods, but defaults to 20 mins.

## Features ##

- Simple API with familiar sliding or absolute expiration
- Guaranteed single evaluation of you factory delegate whose results you want to cache
- Strongly typed generics based API. No need to cast your cached objects every time you retieve them
- Thread safe, concurrency ready
- Interface based API and built in `MockCache` to support test driven development and dependency injection
- Leverages ObjectCache under the hood and can be extended with your own implementation of ObjectCache
- The main class `CachingSevice` is a single class and so could be easily embedded in your application or library
- Good test coverage

## API Documentation

See [the wiki](https://github.com/alastairtree/LazyCache/wiki/LazyCache-API-documentation)

