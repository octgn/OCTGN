namespace Octgn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserSubscriptions",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        SubscriberUserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Id = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false, precision: 0),
                        DateModified = c.DateTime(precision: 0),
                        Category = c.String(unicode: false),
                        UpdateType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.SubscriberUserId })
                .Index(t => new { t.UserId, t.SubscriberUserId }, unique: true, name: "IX_UserIdAndSubscriberUserId");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserSubscriptions", "IX_UserIdAndSubscriberUserId");
            DropTable("dbo.UserSubscriptions");
        }
    }
}
