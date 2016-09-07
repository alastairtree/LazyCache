using System;
using System.Threading.Tasks;
using System.Web.Http;
using LazyCache;
using SampleWebApp.Models;

namespace SampleWebApp.Controllers
{
    public class DbTimeController : ApiController
    {
        private readonly IAppCache cache;
        private readonly string cacheKey = "DbTimeController.Get";
        private readonly DbTimeContext dbContext;


        public DbTimeController()
        {
            // these could be passed in using dependency injection
            dbContext = new DbTimeContext();
            cache = new CachingService();
        }

        [HttpGet]
        [Route("api/dbtime")]
        public async Task<DbTime> Get()
        {
            Func<Task<DbTime>> cacheableAsyncFunc = () => dbContext.GeDbTime();

            var cachedDatabaseTime = await cache.GetOrAddAsync(cacheKey, cacheableAsyncFunc);

            return cachedDatabaseTime;

            // Or instead just do it all in one line if you prefer
            // return await cache.GetOrAddAsync(cacheKey, dbContext.GeDbTime);
        }

        [HttpDelete]
        [Route("api/dbtime")]
        public IHttpActionResult DeleteFromCache()
        {
            cache.Remove(cacheKey);
            var friendlyMessage = new {Message = $"Item with key '{cacheKey}' removed from server in-memory cache"};
            return Ok(friendlyMessage);
        }
    }
}