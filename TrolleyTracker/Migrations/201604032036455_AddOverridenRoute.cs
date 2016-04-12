namespace TrolleyTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOverridenRoute : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RouteScheduleOverrides", "RouteID", "dbo.Routes");
            DropIndex("dbo.RouteScheduleOverrides", new[] { "RouteID" });
            RenameColumn(table: "dbo.RouteScheduleOverrides", name: "RouteID", newName: "NewRouteID");
            AddColumn("dbo.RouteScheduleOverrides", "OverriddenRouteID", c => c.Int());
            AlterColumn("dbo.RouteScheduleOverrides", "NewRouteID", c => c.Int());
            CreateIndex("dbo.RouteScheduleOverrides", "NewRouteID");
            CreateIndex("dbo.RouteScheduleOverrides", "OverriddenRouteID");
            AddForeignKey("dbo.RouteScheduleOverrides", "OverriddenRouteID", "dbo.Routes", "ID");
            AddForeignKey("dbo.RouteScheduleOverrides", "NewRouteID", "dbo.Routes", "ID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RouteScheduleOverrides", "NewRouteID", "dbo.Routes");
            DropForeignKey("dbo.RouteScheduleOverrides", "OverriddenRouteID", "dbo.Routes");
            DropIndex("dbo.RouteScheduleOverrides", new[] { "OverriddenRouteID" });
            DropIndex("dbo.RouteScheduleOverrides", new[] { "NewRouteID" });
            AlterColumn("dbo.RouteScheduleOverrides", "NewRouteID", c => c.Int(nullable: false));
            DropColumn("dbo.RouteScheduleOverrides", "OverriddenRouteID");
            RenameColumn(table: "dbo.RouteScheduleOverrides", name: "NewRouteID", newName: "RouteID");
            CreateIndex("dbo.RouteScheduleOverrides", "RouteID");
            AddForeignKey("dbo.RouteScheduleOverrides", "RouteID", "dbo.Routes", "ID", cascadeDelete: true);
        }
    }
}
