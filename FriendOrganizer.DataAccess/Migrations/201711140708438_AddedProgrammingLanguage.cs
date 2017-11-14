namespace FriendOrganizer.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedProgrammingLanguage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProgrammingLanguages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Friends", "FavoriteLanguageId", c => c.Int());
            CreateIndex("dbo.Friends", "FavoriteLanguageId");
            AddForeignKey("dbo.Friends", "FavoriteLanguageId", "dbo.ProgrammingLanguages", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Friends", "FavoriteLanguageId", "dbo.ProgrammingLanguages");
            DropIndex("dbo.Friends", new[] { "FavoriteLanguageId" });
            DropColumn("dbo.Friends", "FavoriteLanguageId");
            DropTable("dbo.ProgrammingLanguages");
        }
    }
}
