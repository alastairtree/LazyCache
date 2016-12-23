# Release notes for LazyCache #

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