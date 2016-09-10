using System;

namespace ApiAsyncCachingSample.Models
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

        public DateTime TimeNowInTheDatabase { get; set; }


    }
}