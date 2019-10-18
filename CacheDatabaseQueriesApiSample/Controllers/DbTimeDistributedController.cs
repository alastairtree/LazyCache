using System;
using LazyCache;
using Microsoft.AspNetCore.Mvc;

namespace CacheDatabaseQueriesApiSample.Controllers
{
    public class DbTimeDistributedController : Controller
    {
        private readonly IDistributedAppCache _distributedCache;
        private readonly string cacheKey = "DbTimeController.Get";
        private readonly DbTimeContext dbContext;


        public DbTimeDistributedController(DbTimeContext context, IDistributedAppCache distributedCache)
        {
            dbContext = context;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        [Route("api/ddbtime")]
        public DbTimeEntity Get()
        {
            Func<DbTimeEntity> actionThatWeWantToCache = () => dbContext.GeDbTime();

            var cachedDatabaseTime = _distributedCache.GetOrAdd(cacheKey, actionThatWeWantToCache);

            return cachedDatabaseTime;
        }

        [HttpDelete]
        [Route("api/ddbtime")]
        public IActionResult DeleteFromCache()
        {
            _distributedCache.Remove(cacheKey);
            var friendlyMessage = new { Message = $"Item with key '{cacheKey}' removed from server in-memory cache" };
            return Ok(friendlyMessage);
        }
    }
}