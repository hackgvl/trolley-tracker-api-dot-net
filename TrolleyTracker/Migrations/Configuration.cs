namespace TrolleyTracker.Migrations
{
    using System;
    using System.IO;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using System.Text;

    internal sealed class Configuration : DbMigrationsConfiguration<TrolleyTracker.Models.TrolleyTrackerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(TrolleyTracker.Models.TrolleyTrackerContext context)
        {
            //  This method will be called after migrating to the latest version.

            if (context.Trolleys.Count<Models.Trolley>() == 0)
            {
                // Seed database if no data available
                InitialSeedFromScript(context);
            }
            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            //CheckStopArrivalTimeMigration(context);
        }

        /// <summary>
        /// StopArrivalTime migration requires re-calculating stop indices to set
        /// RouteSegmentIndex
        /// </summary>
        /// <param name="context"></param>
        private void CheckStopArrivalTimeMigration(TrolleyTrackerContext context)
        {
            var routeStop = context.RouteStops.FirstOrDefault();
            if (routeStop != null)
            {
                if (routeStop.RouteSegmentIndex < 0)
                {

                    // Need recalculation for all new RouteSegmentIndex fields
                    var routeIDs = (from Route in context.Routes
                                    select Route.ID).ToList();
                    var assignStops = new Controllers.AssignStopsToRoutes();
                    foreach (var routeID in routeIDs)
                    {
                        assignStops.UpdateStopsForRoute(context, routeID);
                    }

                }

            }
        }

        // **** NOT USED - This is handle by normal EF / code first
        //        private void CreateLoggingTableIfNotExist(TrolleyTracker.Models.TrolleyTrackerContext dbContext)
        //        {
        //            dbContext.Database.ExecuteSqlCommand(
        //@"if not exists(select * from sys.tables t where t.name = 'Log')
        //  CREATE TABLE [dbo].[Log] (
        //      [Id] [int] IDENTITY(1,1) NOT NULL,
        //      [Logged] [datetime] NOT NULL,
        //      [Level] [nvarchar](50) NOT NULL,
        //      [Message] [nvarchar](max) NOT NULL,
        //      [UserName] [nvarchar](250) NULL,
        //      [RemoteAddress] [nvarchar](100) NULL,
        //      [Callsite] [nvarchar](max) NULL,
        //      [Exception] [nvarchar](max) NULL,
        //    CONSTRAINT [PK_dbo.Log] PRIMARY KEY CLUSTERED ([Id] ASC)
        //      WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
        //  ) ON [PRIMARY]
        //");
        //        }

        /// <summary>
        /// Populate database with starting dataset.   Note: this script must always be updated so that it can
        /// be applied to the latest model.
        /// </summary>
        /// <param name="context"></param>
        private void InitialSeedFromScript(TrolleyTrackerContext context)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin", string.Empty) + @"\Migrations";
            var filePath = baseDir + "\\InitialTableSeedInserts.sql";
            using (var scriptFile = new StreamReader(filePath))
            {
                var sqlCommand = new StringBuilder();
                while (!scriptFile.EndOfStream)
                {
                    // Series of commands separated by GO
                    var line = scriptFile.ReadLine().Trim();
                    if (line == "GO")
                    {
                        context.Database.ExecuteSqlCommand(sqlCommand.ToString());
                        sqlCommand.Clear();
                    } else
                    {
                        sqlCommand.AppendLine(line);
                    }
                }
                if (sqlCommand.Length > 0)
                {
                    context.Database.ExecuteSqlCommand(sqlCommand.ToString());
                }
            }

        }
    }
}
