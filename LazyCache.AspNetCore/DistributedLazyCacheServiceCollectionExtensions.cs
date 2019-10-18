using System;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
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
            services.TryAddSingleton<IDistributedCache, MemoryDistributedCache>();
            services.TryAddSingleton<IDistributedCacheProvider, DistributedCacheProvider>();
            services.TryAddSingleton<IDistributedAppCache, DistributedCachingService>();

            return services;
        }

        public static IServiceCollection AddDistributedLazyCache(this IServiceCollection services,
            Func<IServiceProvider, DistributedCachingService> implementationFactory)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

            services.AddOptions();
            services.TryAddSingleton<IDistributedCacheProvider, DistributedCacheProvider>();
            services.TryAddSingleton<IDistributedAppCache>(implementationFactory);

            return services;
        }

        public static IServiceCollection AddDistributedHybridLazyCache(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.TryAddSingleton<IMemoryCache, MemoryCache>();
            services.TryAddSingleton<IDistributedCache, MemoryDistributedCache>();
            services.TryAddSingleton<DistributedCacheProvider>();
            services.TryAddSingleton<IDistributedCacheProvider>(provider => new HybridCacheProvider(provider.GetRequiredService<DistributedCacheProvider>(), provider.GetRequiredService<IMemoryCache>()));
            services.TryAddSingleton<IDistributedAppCache, HybridCachingService>();

            return services;
        }

        public static IServiceCollection AddDistributedHybridLazyCache(this IServiceCollection services,
            Func<IServiceProvider, HybridCachingService> implementationFactory)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

            services.AddOptions();
            services.TryAddSingleton<IMemoryCache, MemoryCache>();
            services.TryAddSingleton<DistributedCacheProvider>();
            services.TryAddSingleton<IDistributedCacheProvider>(provider => new HybridCacheProvider(provider.GetRequiredService<DistributedCacheProvider>(), provider.GetRequiredService<IMemoryCache>()));
            services.TryAddSingleton<IDistributedAppCache>(implementationFactory);

            return services;
        }
    }
}
