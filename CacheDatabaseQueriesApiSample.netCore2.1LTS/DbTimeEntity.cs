using System;

namespace CacheDatabaseQueriesApiSample
{
    /// <summary>
    ///     Simulates loading a record from a table, but really just gets the current datatime from the database
    /// </summary>
    public class DbTimeEntity
    {
        public DbTimeEntity(DateTime now)
        {
            TimeNowInTheDatabase = now;
        }

        public DbTimeEntity()
        {
        }

        public virtual int id { get; set; }

        public virtual DateTime TimeNowInTheDatabase { get; set; }
    }
}