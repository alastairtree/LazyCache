using System.Web.Http;
using ApiAsyncCachingSample.Models;

namespace ApiAsyncCachingSample.Controllers
{
    public class DbQueriesController : ApiController
    {
        [HttpGet]
        [Route("api/dbQueries")]
        public int GetDatabaseRequestCounter()
        {
            return DbTimeContext.DatabaseRequestCounter();
        }
    }
}