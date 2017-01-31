namespace TrolleyTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNLog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Logged = c.DateTime(nullable: false),
                        Level = c.String(nullable: false, maxLength: 50),
                        Message = c.String(nullable: false),
                        Username = c.String(maxLength: 250),
                        RemoteAddress = c.String(maxLength: 100),
                        Callsite = c.String(),
                        Exception = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Logs");
        }
    }
}
