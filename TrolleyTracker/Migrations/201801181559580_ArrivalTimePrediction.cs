namespace TrolleyTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArrivalTimePrediction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RouteStops", "RouteSegmentIndex", c => c.Int(nullable: false, defaultValueSql: "-1"));
            AddColumn("dbo.RouteStops", "AverageTravelTimeToNextStop", c => c.Int(nullable: false));
            AddColumn("dbo.RouteStops", "LastTimeAtStop", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RouteStops", "LastTimeAtStop");
            DropColumn("dbo.RouteStops", "AverageTravelTimeToNextStop");
            DropColumn("dbo.RouteStops", "RouteSegmentIndex");
        }
    }
}
