namespace TrolleyTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddScheduleOverrides : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RouteScheduleOverrides",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        RouteID = c.Int(nullable: false),
                        OverrideDate = c.DateTime(nullable: false),
                        OverrideType = c.Int(nullable: false),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Routes", t => t.RouteID, cascadeDelete: true)
                .Index(t => t.RouteID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RouteScheduleOverrides", "RouteID", "dbo.Routes");
            DropIndex("dbo.RouteScheduleOverrides", new[] { "RouteID" });
            DropTable("dbo.RouteScheduleOverrides");
        }
    }
}
