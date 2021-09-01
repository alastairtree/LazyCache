# Release notes for LazyCache #

## Version 2.4.0
- Bug fix for generic type - see #163. Thanks @gamblen
- Bug fix for async wait and abs expiry. Thanks @allanrodriguez
- (Very minor API change that is compiler compatible, but not binary compatible, so incrementing the minor.)

## Version 2.1.3
- Rename ExpirationMode.ImmediateExpiry => ExpirationMode.ImmediateEviction
- Lovely new logo! (#133) Thanks to @doolali
- Performance imporvements to reduce allocations in #134 - thanks @jnyrup

## Version 2.1.2
- Tweak key lock array size based on CPU count so larger for bigger machines (See PR #126 and discussion with @jjxtra)

## Version 2.1.1
- PR #126 (and issue #104) optimise cache key locking for faster performance under high CPU load and lots of concurrency. Thanks to @jjxtra

## Version 2.1.0
- Add options for expiration: 
    - ExpirationMode.ImmediateExpiration which uses a timer to remove items from the cache as soon as they expire (more resource intensive)
    - ExpirationMode.LazyExpiration (existing default) which removes expired cache items when they are next accessed if they have expired.
- Fix #96 AddExpirationToken with CancellationChangeToken is not being honored
- Allow callers to pass MemoryCacheEntryOptions that is used at cache insertion time. This allows users to wire up callbacks and expiration tokens that fire at the correct time

## Version 2.0.5
- Fix #85 and #100 Absolute expiration not working with TimeSpan. Thank you to @Meberem and @Sinhk.
- Fix #124 casing for PackageReference - by @jnyrup 

## Version 2.0.4
- Fix #82 Make constructor resolution  specific when using aspnet core dependency injection

## Version 2.0.3
- Fix #46 GetOrAdd and GetOrAddAsync returning nulll if there is already an item with the same key but different type in the cache (PR #106)

## Version 2.0.2
- Fix #71. Downgrade framework references to core framework 2.1 (not 2.2) since that is the current LTS release (PR #105)

## Version 2.0.1
- Add Ninject adapter (PR #61)

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
