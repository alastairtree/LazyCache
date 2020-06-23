using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;

namespace LazyCache.UnitTestsCore22
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
        public void GetOrAddOnCore22ReturnsTheCachedItem()
        {
            var cachedResult = sut.GetOrAdd(TestKey, () => new {SomeProperty = "SomeValue"});

            Assert.IsNotNull(cachedResult);
            Assert.AreEqual("SomeValue", cachedResult.SomeProperty);
        }
    }
}