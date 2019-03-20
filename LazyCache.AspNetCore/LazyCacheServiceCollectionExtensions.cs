using System;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace - MS guidelines say put DI registration in this NS
namespace Microsoft.Extensions.DependencyInjection
{
    // See https://github.com/aspnet/Caching/blob/dev/src/Microsoft.Extensions.Caching.Memory/MemoryCacheServiceCollectionExtensions.cs
    public static class LazyCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddLazyCache(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<IMemoryCache, MemoryCache>());
            services.TryAdd(ServiceDescriptor.Singleton<ICacheProvider, MemoryCacheProvider>());

            services.TryAdd(ServiceDescriptor.Singleton<IAppCache, CachingService>());

            return services;
        }

        public static IServiceCollection AddLazyCache(this IServiceCollection services,
            Func<IServiceProvider, CachingService> implmentationFactory)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (implmentationFactory == null) throw new ArgumentNullException(nameof(implmentationFactory));

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<IMemoryCache, MemoryCache>());
            services.TryAdd(ServiceDescriptor.Singleton<ICacheProvider, MemoryCacheProvider>());

            services.TryAdd(ServiceDescriptor.Singleton<IAppCache>(implmentationFactory));

            return services;
        }
    }
}