namespace FriendOrganizer.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMeeting : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Meetings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 50),
                        DateFrom = c.DateTime(nullable: false),
                        DateTo = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MeetingFriends",
                c => new
                    {
                        Meeting_Id = c.Int(nullable: false),
                        Friend_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Meeting_Id, t.Friend_Id })
                .ForeignKey("dbo.Meetings", t => t.Meeting_Id, cascadeDelete: true)
                .ForeignKey("dbo.Friends", t => t.Friend_Id, cascadeDelete: true)
                .Index(t => t.Meeting_Id)
                .Index(t => t.Friend_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MeetingFriends", "Friend_Id", "dbo.Friends");
            DropForeignKey("dbo.MeetingFriends", "Meeting_Id", "dbo.Meetings");
            DropIndex("dbo.MeetingFriends", new[] { "Friend_Id" });
            DropIndex("dbo.MeetingFriends", new[] { "Meeting_Id" });
            DropTable("dbo.MeetingFriends");
            DropTable("dbo.Meetings");
        }
    }
}
