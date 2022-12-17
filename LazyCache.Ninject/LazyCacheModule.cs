using System;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Ninject.Modules;

namespace LazyCache
{
    // See https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Caching.Memory/src/MemoryCacheServiceCollectionExtensions.cs
    /// <summary>
    /// Set of extensions for registering LazyCache dependencies with <see cref="IServiceCollection"/> instance.
    /// </summary>
    public class LazyCacheModule : NinjectModule
    {
        private readonly Func<IAppCache> implementationFactory;

        /// <summary>
        /// Initializes new instance of <see cref="LazyCacheModule"/>.
        /// </summary>
        /// <remarks>
        /// For implementation details see <see cref="CachingService"/>
        /// </remarks>
        public LazyCacheModule()
        {
        }

        /// <summary>
        /// Initializes new instance of <see cref="LazyCacheModule"/>.
        /// </summary>
        /// <param name="implementationFactory">A delegate that allows users to inject their own <see cref="IAppCache"/> implementation.</param>
        public LazyCacheModule(Func<IAppCache> implementationFactory)
        {
            this.implementationFactory = implementationFactory;
        }

        /// <summary>
        /// Overrides <see cref="NinjectModule.Load"/> and registers LazyeCache dependencies.
        /// </summary>
        public override void Load()
        {
            Bind<IOptions<MemoryCacheOptions>>().ToConstant(Options.Create(new MemoryCacheOptions()));
            Bind<IMemoryCache>().To<MemoryCache>().InSingletonScope();
            Bind<ICacheProvider>().To<MemoryCacheProvider>().InSingletonScope();

            if (implementationFactory == null)
                Bind<IAppCache>().To<CachingService>().InSingletonScope();
            else
                Bind<IAppCache>().ToMethod(context => implementationFactory()).InSingletonScope();
        }
    }
}