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

        public System.Data.Entity.DbSet<TrolleyTracker.ViewModels.RunningTrolley> RunningTrolleys { get; set; }
    }
}
