namespace TrolleyTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Routes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ShortName = c.String(),
                        LongName = c.String(),
                        Description = c.String(),
                        FlagStopsOnly = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.RouteSchedules",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        RouteID = c.Int(nullable: false),
                        DayOfWeek = c.Int(nullable: false),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Routes", t => t.RouteID, cascadeDelete: true)
                .Index(t => t.RouteID);
            
            CreateTable(
                "dbo.RouteStops",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        RouteID = c.Int(nullable: false),
                        StopID = c.Int(nullable: false),
                        StopSequence = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Routes", t => t.RouteID, cascadeDelete: true)
                .ForeignKey("dbo.Stops", t => t.StopID, cascadeDelete: true)
                .Index(t => t.RouteID)
                .Index(t => t.StopID);
            
            CreateTable(
                "dbo.Stops",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        Lat = c.Double(nullable: false),
                        Lon = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Shapes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        RouteID = c.Int(nullable: false),
                        Lat = c.Double(nullable: false),
                        Lon = c.Double(nullable: false),
                        Sequence = c.Int(nullable: false),
                        DistanceTraveled = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Routes", t => t.RouteID, cascadeDelete: true)
                .Index(t => t.RouteID);
            
            CreateTable(
                "dbo.RunningTrolleys",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Lat = c.Double(nullable: false),
                        Lon = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Trolleys",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TrolleyName = c.String(),
                        Number = c.Int(nullable: false),
                        CurrentLat = c.Double(),
                        CurrentLon = c.Double(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Shapes", "RouteID", "dbo.Routes");
            DropForeignKey("dbo.RouteStops", "StopID", "dbo.Stops");
            DropForeignKey("dbo.RouteStops", "RouteID", "dbo.Routes");
            DropForeignKey("dbo.RouteSchedules", "RouteID", "dbo.Routes");
            DropIndex("dbo.Shapes", new[] { "RouteID" });
            DropIndex("dbo.RouteStops", new[] { "StopID" });
            DropIndex("dbo.RouteStops", new[] { "RouteID" });
            DropIndex("dbo.RouteSchedules", new[] { "RouteID" });
            DropTable("dbo.Trolleys");
            DropTable("dbo.RunningTrolleys");
            DropTable("dbo.Shapes");
            DropTable("dbo.Stops");
            DropTable("dbo.RouteStops");
            DropTable("dbo.RouteSchedules");
            DropTable("dbo.Routes");
        }
    }
}
