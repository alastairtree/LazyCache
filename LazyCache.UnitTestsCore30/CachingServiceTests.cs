using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using System.Threading.Tasks;
using System;

namespace LazyCache.UnitTestsCore30
{
    [TestFixture]
    public class CachingServiceTests
    {
        private static CachingService BuildCache()
        {
            return new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())));
        }

        private IAppCache sut;


        private const string TestKey = "testKey";

        [SetUp]
        public void BeforeEachTest()
        {
            sut = BuildCache();
        }


        [Test]
        public void GetOrAddOnCore30ReturnsTheCachedItem()
        {
            var cachedResult = sut.GetOrAdd(TestKey, () => new {SomeProperty = "SomeValue"});

            Assert.IsNotNull(cachedResult);
            Assert.AreEqual("SomeValue", cachedResult.SomeProperty);
        }

        [Test]
        public async Task GetOrAddAsyncOnCore30DefaultCacheDurationHonoured()
        {
            sut.DefaultCachePolicy.DefaultCacheDurationSeconds = 1;

            int value = DateTime.UtcNow.Second;
            int result = await sut.GetOrAddAsync("foo", x => Task.FromResult(value));

            Assert.AreEqual(value, result);

            // wait for the item to expire
            await Task.Delay(TimeSpan.FromSeconds(2));

            // same key
            value = DateTime.UtcNow.Second;
            result = await sut.GetOrAddAsync("foo", x => Task.FromResult(value));
            Assert.AreEqual(value, result);

            // new key
            value = DateTime.UtcNow.Second;
            result = await sut.GetOrAddAsync("bar", x => Task.FromResult(value));
            Assert.AreEqual(value, result);
        }

        [Test]
        public async Task GetOrAddOnCore30DefaultCacheDurationHonoured()
        {
            sut.DefaultCachePolicy.DefaultCacheDurationSeconds = 1;

            int value = DateTime.UtcNow.Second;
            int result = await sut.GetOrAdd("foo", x => Task.FromResult(value));

            Assert.AreEqual(value, result);

            // wait for the item to expire
            await Task.Delay(TimeSpan.FromSeconds(2));

            // same key
            value = DateTime.UtcNow.Second;
            result = await sut.GetOrAdd("foo", x => Task.FromResult(value));
            Assert.AreEqual(value, result);

            // new key
            value = DateTime.UtcNow.Second;
            result = await sut.GetOrAdd("bar", x => Task.FromResult(value));
            Assert.AreEqual(value, result);
        }
    }
}