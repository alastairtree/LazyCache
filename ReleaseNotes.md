# Release notes for LazyCache #

## Version 2.0.0
- *BREAKING CHANGE* Upgrade to netstandard2.0
- *BREAKING CHANGE* Change underlying cache from System.Runtime.Caching to Microsft.Extension.Caching.Memory
- *BREAKING CHANGE* Removed IAppCache.ObjectCache and changed to a cache provider model. 
  To access the provider use IAppCache.CacheProvider. By default we use a singleton shared in-memory cache but add your own cache provider by implmenting the simple `ICacheProvider`.
- *BREAKING CHANGE* changed from CacheItemPolicy to MemoryCacheEntryOptions. RemovedCallback is now PostEvictionCallbacks.
- Added a new replaceable global static default cache provider 
    
  `Func<ICacheProvider> DefaultCacheProvider { get; }`
  
  By default we use a shared in-memory cache but each instance can have it's underlying cache provider overridden from it's constructor.
- Make methods on CachingService virtual/protected to enable 
- Add LazyCache.AspNetCore for dependency injection registration - ServiceCollection.AddLazyCache();
- Update sample to use aspnet core and LazyCache.AspNetCore
- New IAppCache.DefaultCachePolicy to replace CachingService.DefaultCacheDuration
- Moved most CachingService method overloads to extension methods on IAppCache in AppCacheExtensions. API should be backwards compatible but as now extension methods this is technically an API breaking changing.
- Added new methods on IAppCache to allow you to specify cache expiry options on executution of the item factory
   
  `GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory)`
  
  `Task<T> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory)`


## Version 0.7.1
- Fix async/sync interopability bug, see https://github.com/alastairtree/LazyCache/issues/12

## Version 0.7

- *BREAKING CHANGE* Upgrade to .net 4.5
- Added ObjectCache property to IAppCache to allow access to underlying cache for operations such as cache clearing
- Support caching asynchronous tasks with GetOrAddAsync methods
- Add ApiAsyncCachingSample to demonstrate the caching the results of SQL Queries in a WebApi controller
- Add badges to Readme

## Version 0.6

- Fixed issue with RemovedCallback not unwrapping the Lazy used to thread safe the cache item.

## Version 0.5

- Initial release of CachingService and interface IAppCache. 
- Readme
- Core unit tests.