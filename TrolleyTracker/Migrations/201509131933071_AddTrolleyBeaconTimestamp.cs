namespace TrolleyTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTrolleyBeaconTimestamp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Trolleys", "LastBeaconTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Trolleys", "LastBeaconTime");
        }
    }
}
