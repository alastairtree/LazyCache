using System;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
            services.TryAdd(ServiceDescriptor.Singleton<ICacheProvider, MemoryCacheProvider>(serviceProvider => 
                new MemoryCacheProvider(() => new MemoryCache(new MemoryCacheOptions()))));

            services.TryAdd(ServiceDescriptor.Singleton<IAppCache, CachingService>());

            return services;
        }

        public static IServiceCollection AddLazyCache(this IServiceCollection services,
            Func<IServiceProvider, CachingService> implementationFactory)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<ICacheProvider, MemoryCacheProvider>(serviceProvider =>
                new MemoryCacheProvider(() => new MemoryCache(new MemoryCacheOptions()))));

            services.TryAdd(ServiceDescriptor.Singleton<IAppCache>(implementationFactory));

            return services;
        }
    }
}
