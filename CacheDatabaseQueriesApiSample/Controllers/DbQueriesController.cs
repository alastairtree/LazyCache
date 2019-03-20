using CacheDatabaseQueriesApiSample;
using Microsoft.AspNetCore.Mvc;

namespace ApiAsyncCachingSample.Controllers
{
    public class DbQueriesController : Controller
    {
        [HttpGet]
        [Route("api/dbQueries")]
        public int GetDatabaseRequestCounter()
        {
            return DbTimeContext.DatabaseRequestCounter();
        }
    }
}