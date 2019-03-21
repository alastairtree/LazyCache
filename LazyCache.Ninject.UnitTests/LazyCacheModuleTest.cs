using Ninject;
using NUnit.Framework;

namespace LazyCache.Ninject.UnitTests
{
    public class LazyCacheNinjectModuleTests
    {
        [Test]
        public void CanCreateCache()
        {
            IKernel kernel = new StandardKernel(new LazyCacheModule());
            IAppCache cache = kernel.Get<IAppCache>();

            var cached = cache.GetOrAdd("some-key", () => new object());

            Assert.NotNull(cached);
        }
    }
}