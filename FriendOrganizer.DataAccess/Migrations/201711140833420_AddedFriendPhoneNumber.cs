namespace FriendOrganizer.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFriendPhoneNumber : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FriendPhoneNumbers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Number = c.String(nullable: false),
                        FriendId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Friends", t => t.FriendId, cascadeDelete: true)
                .Index(t => t.FriendId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FriendPhoneNumbers", "FriendId", "dbo.Friends");
            DropIndex("dbo.FriendPhoneNumbers", new[] { "FriendId" });
            DropTable("dbo.FriendPhoneNumbers");
        }
    }
}
