using System;

namespace CacheDatabaseQueriesApiSample
{
    public class DbTime
    {
        public DbTime(DateTime now)
        {
            TimeNowInTheDatabase = now;
        }

        public DbTime()
        {
            
        }

        public virtual int id { get; set; }

        public virtual DateTime TimeNowInTheDatabase { get; set; }


    }
}