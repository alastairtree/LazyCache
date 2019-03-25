using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CacheDatabaseQueriesApiSample
{
    public class DbTimeContext : DbContext
    {
        private static int databaseRequestCounter; //just for demo - don't use static fields for statistics!

        public DbTimeContext(DbContextOptions<DbTimeContext> options)
            : base(options)
        {
        }

        // simulate a table in the database so we can get just one row with the current time
        private DbSet<DbTimeEntity> Times { get; set; }

        public static int DatabaseRequestCounter()
        {
            return databaseRequestCounter;
        }

        public DbTimeEntity GeDbTime()
        {
            // get the current time from SQL server right now asynchronously (simulating a slow query)
            var result = Times
                .FromSql("WAITFOR DELAY '00:00:00:500'; SELECT 1 as [ID], GETDATE() as [TimeNowInTheDatabase]")
                .SingleOrDefault();

            databaseRequestCounter++;

            return result;
        }
    }
}