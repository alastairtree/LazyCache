using System.Web.Http;
using SampleWebApp.Models;

namespace SampleWebApp.Controllers
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