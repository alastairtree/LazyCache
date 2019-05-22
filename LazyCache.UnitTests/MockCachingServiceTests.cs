using System;
using FluentAssertions;
using LazyCache.Mocks;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;

namespace LazyCache.UnitTests
{
    public class MockCachingServiceTests
    {
        private const string CacheKey = "cacheKey";
        private const string FunctionReturnValue = "someValue";

        [Test]
        public void GetOrAddCalledWithCacheKeyAndFunction_FunctionResultReturned()
        {
            var sut = new MockCachingService();

            var result = sut.GetOrAdd(CacheKey, () => FunctionReturnValue);

            result.Should().Be(FunctionReturnValue);
        }

        [Test]
        public void GetOrAddCalledWithCacheKeyAndFunctionWithCacheEntry_FunctionResultReturned()
        {
            var sut = new MockCachingService();

            var result = sut.GetOrAdd(CacheKey, cacheEntry => FunctionReturnValue);

            result.Should().Be(FunctionReturnValue);
        }

        [Test]
        public void GetOrAddCalledWithCacheKeyAndFunctionWithDateTimeOffSet_FunctionResultReturned()
        {
            var sut = new MockCachingService();

            var result = sut.GetOrAdd(CacheKey, () => FunctionReturnValue, DateTimeOffset.MinValue);

            result.Should().Be(FunctionReturnValue);
        }

        [Test]
        public void GetOrAddCalledWithCacheKeyAndFunctionWithMemoryCacheEntryOptions_FunctionResultReturned()
        {
            var sut = new MockCachingService();

            var result = sut.GetOrAdd(CacheKey, () => FunctionReturnValue, new MemoryCacheEntryOptions());

            result.Should().Be(FunctionReturnValue);
        }

        [Test]
        public void GetOrAddCalledWithCacheKeyAndFunctionWithTimeSpan_FunctionResultReturned()
        {
            var sut = new MockCachingService();

            var result = sut.GetOrAdd(CacheKey, () => FunctionReturnValue, TimeSpan.FromMilliseconds(1000));

            result.Should().Be(FunctionReturnValue);
        }
    }
}
