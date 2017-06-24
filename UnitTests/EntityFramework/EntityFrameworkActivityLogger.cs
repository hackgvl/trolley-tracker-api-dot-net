using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;

namespace UnitTests.EntityFramework
{
    public class EntityFrameworkActivityLogger : IDbCommandInterceptor
    {
        public int TotalExecutedCount { get; private set; }
        public int NonQueryExecutedCount { get; private set; }
        public int ReaderExecutedCount { get; private set; }
        public int ScalarExecutedCount { get; private set; }

        public EntityFrameworkActivityLogger()
        {
            Reset();
        }

        public void Reset()
        {
            TotalExecutedCount = 0;
            NonQueryExecutedCount = 0;
            ReaderExecutedCount = 0;
            ScalarExecutedCount = 0;
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            TotalExecutedCount += 1;
            NonQueryExecutedCount += 1;
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            TotalExecutedCount += 1;
            ReaderExecutedCount += 1;
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            TotalExecutedCount += 1;
            ScalarExecutedCount += 1;
        }


        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            // Not logged
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            // Not logged
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            // Not logged
        }
    }
}
