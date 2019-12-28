using System;
using LazyCache;
using Ninject;

namespace Console.Net461
{
    class Program
    {
        static void Main()
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
        }
    }
}