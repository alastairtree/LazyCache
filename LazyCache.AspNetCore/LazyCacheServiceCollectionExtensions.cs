using System;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace - MS guidelines say put DI registration in this NS
namespace Microsoft.Extensions.DependencyInjection
{
    // See https://github.com/dotnet/runtime/blob/master/src/libraries/Microsoft.Extensions.Caching.Memory/src/MemoryCacheServiceCollectionExtensions.cs
    public static class LazyCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddLazyCache(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<IMemoryCache, MemoryCache>());
            services.TryAdd(ServiceDescriptor.Singleton<ICacheProvider, MemoryCacheProvider>());

            services.TryAdd(ServiceDescriptor.Singleton<IAppCache, CachingService>(serviceProvider => 
                new CachingService(
                    new Lazy<ICacheProvider>(serviceProvider.GetRequiredService<ICacheProvider>))));

            return services;
        }

        public static IServiceCollection AddLazyCache(this IServiceCollection services,
            Func<IServiceProvider, CachingService> implementationFactory)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<IMemoryCache, MemoryCache>());
            services.TryAdd(ServiceDescriptor.Singleton<ICacheProvider, MemoryCacheProvider>());

            services.TryAdd(ServiceDescriptor.Singleton<IAppCache>(implementationFactory));

            return services;
        }
    }
}