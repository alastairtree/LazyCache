using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace LazyCache.UnitTests
{
    public class AspNetCoreTests
    {
        [Test]
        public void CanResolveCacheFromServiceCollectionAsRequiredService()
        {
            var container = new ServiceCollection();
            container.AddLazyCache();
            var provider = container.BuildServiceProvider();

            var cache = provider.GetRequiredService<IAppCache>();
            var result = cache?.GetOrAdd("key", () => new object());

            cache.Should().NotBeNull();
            result.Should().NotBeNull();
        }

        [Test]
        public void CanResolveCacheFromServiceCollectionAsService()
        {
            var container = new ServiceCollection();
            container.AddLazyCache();
            var provider = container.BuildServiceProvider();

            var cache = provider.GetService<IAppCache>();
            var result = cache?.GetOrAdd("key", () => new object());

            cache.Should().NotBeNull();
            result.Should().NotBeNull();
        }
        
        [Test]
        public void CanResolveCacheFromServiceCollectionWithOptionsAsService()
        {
            var container = new ServiceCollection();
            var cacheDurationSeconds = 12345;
            container.AddLazyCache(options => options.DefaultCacheDurationSeconds = cacheDurationSeconds);
            var provider = container.BuildServiceProvider();

            var cache = provider.GetService<IAppCache>();
            var result = cache?.GetOrAdd("key", () => new object());

            cache.Should().NotBeNull();
            cache.DefaultCachePolicy.DefaultCacheDurationSeconds.Should().Be(cacheDurationSeconds);
            result.Should().NotBeNull();
        }
    }
}
