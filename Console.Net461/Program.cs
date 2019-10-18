using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Ninject;

namespace Console.Net461
{
    class Program
    {
        static void Main(string[] args)
        {
            //check one - basic LazyCache
            IAppCache cache = new CachingService(CachingService.DefaultCacheProvider);

            var item = cache.GetOrAdd("Program.Main.Person", () => Tuple.Create("Joe Blogs", DateTime.UtcNow));

            System.Console.WriteLine(item.Item1);

            //check two - using Ninject
            IKernel kernel = new StandardKernel(new LazyCacheModule());
            cache = kernel.Get<IAppCache>();

            item = cache.GetOrAdd("Program.Main.Person", () => Tuple.Create("Joe Blogs", DateTime.UtcNow));

            System.Console.WriteLine(item.Item1);

            IDistributedAppCache distributedCache = new HybridCachingService(new HybridCacheProvider(new DistributedCacheProvider(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()))), new MemoryCache(Options.Create(new MemoryCacheOptions()))));
            item = distributedCache.GetOrAdd("Program.Main.Person", () => Tuple.Create("Joe Blogs", DateTime.UtcNow));

            System.Console.WriteLine(item.Item1);
        }
    }
}
