namespace TrolleyTracker.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TrolleyTrackerContext : DbContext
    {
        public TrolleyTrackerContext()
            : base("name=TrolleyTrackerContext")
        {

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TrolleyTrackerContext, TrolleyTracker.Migrations.Configuration>());

        }

        public virtual DbSet<Route> Routes { get; set; }
        public virtual DbSet<RouteStop> RouteStops { get; set; }
        public virtual DbSet<Shape> Shapes { get; set; }
        public virtual DbSet<Stop> Stops { get; set; }
        public virtual DbSet<Trolley> Trolleys { get; set; }
        public virtual DbSet<RouteSchedule> RouteSchedules { get; set; }
        public virtual DbSet<RouteScheduleOverride> RouteScheduleOverrides { get; set; }

        public System.Data.Entity.DbSet<TrolleyTracker.ViewModels.RunningTrolley> RunningTrolleys { get; set; }

        // NLog writes to this table with direct SQL
        public virtual DbSet<Log> Logs { get; set; }

        public System.Data.Entity.DbSet<TrolleyTracker.Models.AppSettings> AppSettings { get; set; }
    }
}
