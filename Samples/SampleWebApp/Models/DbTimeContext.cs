using System.Data.Entity;
using System.Threading.Tasks;

namespace SampleWebApp.Models
{
    public class DbTimeContext : DbContext
    {
        private static int databaseRequestCounter = 0; //just for demo - don't use static fields for statistics!

        public static int DatabaseRequestCounter()
        {
            return databaseRequestCounter;
        }

        public async Task<DbTime> GeDbTime()
        {
            // get the current time from SQL server right now asynchronously (simulating a slow query)
            var result = await Database
                .SqlQuery<DbTime>("WAITFOR DELAY '00:00:00:500'; SELECT GETDATE() as [TimeNowInTheDatabase]")
                .SingleAsync();

            databaseRequestCounter++;

            return result;
        }
    }
}