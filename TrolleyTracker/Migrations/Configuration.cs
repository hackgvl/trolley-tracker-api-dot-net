namespace TrolleyTracker.Migrations
{
    using System;
    using System.IO;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Models;
    using System.Text;

    internal sealed class Configuration : DbMigrationsConfiguration<TrolleyTracker.Models.TrolleyTrackerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
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
        }

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
