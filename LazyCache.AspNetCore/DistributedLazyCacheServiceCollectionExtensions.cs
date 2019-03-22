using System;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace - MS guidelines say put DI registration in this NS
namespace Microsoft.Extensions.DependencyInjection
{
    // See https://github.com/aspnet/Caching/blob/dev/src/Microsoft.Extensions.Caching.Memory/MemoryCacheServiceCollectionExtensions.cs
    public static class DistributedLazyCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedLazyCache(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<IDistributedCache, MemoryDistributedCache>());
            services.TryAdd(ServiceDescriptor.Singleton<IDistributedCacheProvider, DistributedCacheProvider>());

            services.TryAdd(ServiceDescriptor.Singleton<IAppCache, CachingService>());

            return services;
        }

        public static IServiceCollection AddDistributedLazyCache(this IServiceCollection services,
            Func<IServiceProvider, DistributedCachingService> implementationFactory)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<IDistributedCacheProvider, DistributedCacheProvider>());

            services.TryAdd(ServiceDescriptor.Singleton<IDistributedAppCache>(implementationFactory));

            return services;
        }
    }
}
