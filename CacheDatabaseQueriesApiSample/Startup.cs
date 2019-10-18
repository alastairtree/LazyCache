﻿using LazyCache;
using LazyCache.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CacheDatabaseQueriesApiSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // just for demo - use app settings for db config
            var connection =
                @"Server=(localdb)\projectsv13;Database=Master;Trusted_Connection=True;ConnectRetryCount=0";

            // register the database
            services.AddDbContext<DbTimeContext>(options => options.UseSqlServer(connection));

            // Register IAppCache as a singleton CachingService
            services.AddLazyCache();

            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddSingleton<DistributedCacheProvider>();
            services.AddTransient<IDistributedCacheProvider>(provider => new HybridCacheProvider(provider.GetRequiredService<DistributedCacheProvider>(), provider.GetRequiredService<IMemoryCache>()));
            services.AddDistributedHybridLazyCache(provider => new HybridCachingService(provider.GetRequiredService<IDistributedCacheProvider>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}