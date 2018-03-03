using System;
using LazyCache;

// ReSharper disable once CheckNamespace - MS guidelines say put DI registration in this NS
namespace Microsoft.Extensions.DependencyInjection
{
    public static class LazyCacheServiceRegistration
    {
        public static IServiceCollection AddLazyCache(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IAppCache, CachingService>();

            return services;
        }

        public static IServiceCollection AddLazyCache(this IServiceCollection services, Func<IServiceProvider, CachingService> implmentationFactory)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (implmentationFactory == null) throw new ArgumentNullException(nameof(implmentationFactory));

            services.AddSingleton<IAppCache>(implmentationFactory);

            return services;
        }
    }
}
