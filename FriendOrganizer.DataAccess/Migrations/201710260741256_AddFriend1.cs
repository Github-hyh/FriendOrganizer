namespace FriendOrganizer.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFriend1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Friends", "FirstName", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Friends", "LastName", c => c.String(maxLength: 50));
            AlterColumn("dbo.Friends", "Email", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Friends", "Email", c => c.String(maxLength: 100));
            AlterColumn("dbo.Friends", "LastName", c => c.String(maxLength: 100));
            AlterColumn("dbo.Friends", "FirstName", c => c.String(nullable: false, maxLength: 100));
        }
    }
}
