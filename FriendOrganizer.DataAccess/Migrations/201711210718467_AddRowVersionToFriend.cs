namespace FriendOrganizer.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRowVersionToFriend : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Friends", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Friends", "RowVersion");
        }
    }
}
