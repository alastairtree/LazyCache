using System;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Ninject;
using Ninject.Modules;

namespace LazyCache
{
    public class LazyCacheModule : NinjectModule
    {
        private readonly Func<IAppCache> implementationFactory;

        public LazyCacheModule()
        {
        }

        public LazyCacheModule(Func<IAppCache> implementationFactory)
        {
            this.implementationFactory = implementationFactory;
        }

        // See also https://github.com/aspnet/Caching/blob/dev/src/Microsoft.Extensions.Caching.Memory/MemoryCacheServiceCollectionExtensions.cs
        public override void Load()
        {
            Bind<IOptions<MemoryCacheOptions>>().ToConstant(Options.Create(new MemoryCacheOptions()));
            Bind<ICacheProvider>().To<MemoryCacheProvider>().InSingletonScope()
                .WithConstructorArgument<Func<IMemoryCache>>(context => () => new MemoryCache(context.Kernel.Get<IOptions<MemoryCacheOptions>>()));

            if (implementationFactory == null)
                Bind<IAppCache>().To<CachingService>().InSingletonScope();
            else
                Bind<IAppCache>().ToMethod(context => implementationFactory()).InSingletonScope();
        }
    }
}