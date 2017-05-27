namespace TrolleyTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrolleyColor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Trolleys", "IconColorRGB", c => c.String(nullable: false, maxLength: 9, defaultValueSql: "'#000080'"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Trolleys", "IconColorRGB");
        }
    }
}
