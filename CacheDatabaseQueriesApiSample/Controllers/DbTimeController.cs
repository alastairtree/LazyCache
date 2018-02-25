using System;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.AspNetCore.Mvc;

namespace CacheDatabaseQueriesApiSample.Controllers
{
    public class DbTimeController : Controller
    {
        private readonly IAppCache cache;
        private readonly string cacheKey = "DbTimeController.Get";
        private readonly DbTimeContext dbContext;


        public DbTimeController(DbTimeContext context)
        {
            // this could (and should) be passed in using dependency injection
            cache = new CachingService();
            dbContext = context;
        }

        [HttpGet]
        [Route("api/dbtime")]
        public DbTime Get()
        {
            Func<DbTime> cacheableAsyncFunc = () => dbContext.GeDbTime();

            var cachedDatabaseTime = cache.GetOrAdd(cacheKey, cacheableAsyncFunc);

            return cachedDatabaseTime;
        }

        [HttpDelete]
        [Route("api/dbtime")]
        public IActionResult DeleteFromCache()
        {
            cache.Remove(cacheKey);
            var friendlyMessage = new {Message = $"Item with key '{cacheKey}' removed from server in-memory cache"};
            return Ok(friendlyMessage);
        }
    }
}