namespace TrolleyTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StopPhoto_requiredFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stops", "Picture", c => c.Binary(storeType: "image"));
            AlterColumn("dbo.Stops", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Stops", "Description", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Stops", "Description", c => c.String());
            AlterColumn("dbo.Stops", "Name", c => c.String());
            DropColumn("dbo.Stops", "Picture");
        }
    }
}
