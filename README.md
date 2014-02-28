# Lazy Cache #

Lazy cache is a simple in-memory caching service. It has a developer friendly generics based API, and providing a thread safe cache implementation that  guarantees to only execute your cachable delegates once (it's lazy!). Under the hood it leverages ObjectCache and Lazy<T> to provide performance and reliability in heavy load scenarios

## Example ##

    // Declare (but don't execute) a func whose result we want to cache
    Func<ComplexObects> complexObjectFactory = () => methodThatTakesTimeOrResources();
    
    // Get hold of the cache (Dependency injection would be better).
    // Uses MemoryCache.Default under the hood so cache is shared
    IAppCache cache = new CachingService();
    
    // If we have generated the complexObject recently return the 
    // cached instances, otherwise build and cache them for later
    ComplexObject cachedResults = cache.GetOrAdd("uniqueKey", complexObjectFactory);
    
As you can see the magic happens in the `GetOrAdd()` method which gives the consumer an atomic and tidy way to add caching to your code, with a delegate func to optionally invoke the code for slow results you want to cache. It means you avoid the usual "Check the cache - generate - add to the cache" pattern and can be a lazy developer!

## Use case ##

Suits the caching of database calls, complex object graph building routines and web service calls that should be cached for performance. Allows items to be cached for long or short periods, but defaults to 20 mins.

## Features ##

- Simple API with easy sliding and absolute expiration
- Guaranteed single evaluation of you long running code whose results you want to cache
- Strongly typed generics based API. No need to cast your cached objects every time
- Thread safe, concurrency ready
- Leverages ObjectCache under the hood and can be extended with your own implementation of the ObjectCache object
- Good test coverage
- `CachingSevice` is a single class and so could be simply embedded
- Interface based API and built in `MockCache` to support test driven development

## To do ##
- Create a nuget package for LazyCache
