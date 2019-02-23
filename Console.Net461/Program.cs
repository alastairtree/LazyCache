using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyCache;

namespace Console.Net461
{
    class Program
    {
        static void Main(string[] args)
        {
            IAppCache cache = new CachingService(CachingService.DefaultCacheProvider);

            var item = cache.GetOrAdd("Program.Main.Person", () => Tuple.Create("Joe Blogs", DateTime.UtcNow));

            System.Console.WriteLine(item.Item1);
        }
    }
}
