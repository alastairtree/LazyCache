using System;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace - MS guidelines say put DI registration in this NS
namespace Microsoft.Extensions.DependencyInjection
{
    // See https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Caching.Memory/src/MemoryCacheServiceCollectionExtensions.cs
    /// <summary>
    /// Set of extensions for registering LazyCache dependencies with <see cref="IServiceCollection"/> instance.
    /// </summary>
    public static class LazyCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Register a non distributed in memory implementation of <see cref="IAppCache"/>.
        /// </summary>
        /// <remarks>
        /// For implementation details see <see cref="CachingService"/>
        /// </remarks>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <returns>Modified instance of <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>
        /// Register a custom implementation of <see cref="IAppCache"/> to the
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="implementationFactory">A delegate that allows users to inject their own <see cref="IAppCache"/> implementation.</param>
        /// <returns>Modified instance of <see cref="IServiceCollection"/.></returns>
        /// <exception cref="ArgumentNullException">Thrown when any of: <paramref name="services"/> or <paramref name="implementationFactory"/> are null.</exception>
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