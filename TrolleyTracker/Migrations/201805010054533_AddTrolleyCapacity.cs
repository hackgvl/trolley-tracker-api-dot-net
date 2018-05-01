namespace TrolleyTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTrolleyCapacity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Trolleys", "SyncromaticsNumber", c => c.Int(nullable: false, defaultValueSql: "0"));
            AddColumn("dbo.Trolleys", "Capacity", c => c.Int(nullable: false, defaultValueSql: "0"));
            AddColumn("dbo.Trolleys", "PassengerLoad", c => c.Double(nullable: false, defaultValueSql: "0.0"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Trolleys", "PassengerLoad");
            DropColumn("dbo.Trolleys", "Capacity");
            DropColumn("dbo.Trolleys", "SyncromaticsNumber");
        }
    }
}
