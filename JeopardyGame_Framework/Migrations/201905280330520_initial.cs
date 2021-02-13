namespace JeopardyGame_Framework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BaseGameModels",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        BaseGameShowNumber = c.String(),
                        BaseGameJArchiveGameId = c.String(),
                        BaseGameShowDebutDate = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.GameModels",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        GameFinished = c.Boolean(nullable: false),
                        ShowNumber = c.String(),
                        JArchiveGameId = c.String(),
                        ShowDebutDate = c.String(),
                        GameScore = c.String(),
                        UserEmail = c.String(),
                        GameTally = c.String(),
                        DatePlayed = c.String(),
                        DJRound_ID = c.Int(),
                        FJRound_ID = c.Int(),
                        JRound_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DoubleJeopardies", t => t.DJRound_ID)
                .ForeignKey("dbo.FinalJeopardies", t => t.FJRound_ID)
                .ForeignKey("dbo.Jeopardies", t => t.JRound_ID)
                .Index(t => t.DJRound_ID)
                .Index(t => t.FJRound_ID)
                .Index(t => t.JRound_ID);
            
            CreateTable(
                "dbo.DoubleJeopardies",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        DoubleJeopardyScore = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Answers",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ClueAnswer = c.String(),
                        DoubleJeopardy_ID = c.Int(),
                        Jeopardy_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DoubleJeopardies", t => t.DoubleJeopardy_ID)
                .ForeignKey("dbo.Jeopardies", t => t.Jeopardy_ID)
                .Index(t => t.DoubleJeopardy_ID)
                .Index(t => t.Jeopardy_ID);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(),
                        DoubleJeopardy_ID = c.Int(),
                        Jeopardy_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DoubleJeopardies", t => t.DoubleJeopardy_ID)
                .ForeignKey("dbo.Jeopardies", t => t.Jeopardy_ID)
                .Index(t => t.DoubleJeopardy_ID)
                .Index(t => t.Jeopardy_ID);
            
            CreateTable(
                "dbo.Clues",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ClueText = c.String(),
                        DoubleJeopardy_ID = c.Int(),
                        Jeopardy_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DoubleJeopardies", t => t.DoubleJeopardy_ID)
                .ForeignKey("dbo.Jeopardies", t => t.Jeopardy_ID)
                .Index(t => t.DoubleJeopardy_ID)
                .Index(t => t.Jeopardy_ID);
            
            CreateTable(
                "dbo.Positions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CluePosition = c.String(),
                        DoubleJeopardy_ID = c.Int(),
                        Jeopardy_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DoubleJeopardies", t => t.DoubleJeopardy_ID)
                .ForeignKey("dbo.Jeopardies", t => t.Jeopardy_ID)
                .Index(t => t.DoubleJeopardy_ID)
                .Index(t => t.Jeopardy_ID);
            
            CreateTable(
                "dbo.Values",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ClueValue = c.String(),
                        DoubleJeopardy_ID = c.Int(),
                        Jeopardy_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DoubleJeopardies", t => t.DoubleJeopardy_ID)
                .ForeignKey("dbo.Jeopardies", t => t.Jeopardy_ID)
                .Index(t => t.DoubleJeopardy_ID)
                .Index(t => t.Jeopardy_ID);
            
            CreateTable(
                "dbo.FinalJeopardies",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        FinalJeopardyScore = c.Int(nullable: false),
                        FJClue = c.String(),
                        FJAnswer = c.String(),
                        FJCategory = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Jeopardies",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        JeopardyScore = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.GameModels", "JRound_ID", "dbo.Jeopardies");
            DropForeignKey("dbo.Values", "Jeopardy_ID", "dbo.Jeopardies");
            DropForeignKey("dbo.Positions", "Jeopardy_ID", "dbo.Jeopardies");
            DropForeignKey("dbo.Clues", "Jeopardy_ID", "dbo.Jeopardies");
            DropForeignKey("dbo.Categories", "Jeopardy_ID", "dbo.Jeopardies");
            DropForeignKey("dbo.Answers", "Jeopardy_ID", "dbo.Jeopardies");
            DropForeignKey("dbo.GameModels", "FJRound_ID", "dbo.FinalJeopardies");
            DropForeignKey("dbo.GameModels", "DJRound_ID", "dbo.DoubleJeopardies");
            DropForeignKey("dbo.Values", "DoubleJeopardy_ID", "dbo.DoubleJeopardies");
            DropForeignKey("dbo.Positions", "DoubleJeopardy_ID", "dbo.DoubleJeopardies");
            DropForeignKey("dbo.Clues", "DoubleJeopardy_ID", "dbo.DoubleJeopardies");
            DropForeignKey("dbo.Categories", "DoubleJeopardy_ID", "dbo.DoubleJeopardies");
            DropForeignKey("dbo.Answers", "DoubleJeopardy_ID", "dbo.DoubleJeopardies");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Values", new[] { "Jeopardy_ID" });
            DropIndex("dbo.Values", new[] { "DoubleJeopardy_ID" });
            DropIndex("dbo.Positions", new[] { "Jeopardy_ID" });
            DropIndex("dbo.Positions", new[] { "DoubleJeopardy_ID" });
            DropIndex("dbo.Clues", new[] { "Jeopardy_ID" });
            DropIndex("dbo.Clues", new[] { "DoubleJeopardy_ID" });
            DropIndex("dbo.Categories", new[] { "Jeopardy_ID" });
            DropIndex("dbo.Categories", new[] { "DoubleJeopardy_ID" });
            DropIndex("dbo.Answers", new[] { "Jeopardy_ID" });
            DropIndex("dbo.Answers", new[] { "DoubleJeopardy_ID" });
            DropIndex("dbo.GameModels", new[] { "JRound_ID" });
            DropIndex("dbo.GameModels", new[] { "FJRound_ID" });
            DropIndex("dbo.GameModels", new[] { "DJRound_ID" });
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Jeopardies");
            DropTable("dbo.FinalJeopardies");
            DropTable("dbo.Values");
            DropTable("dbo.Positions");
            DropTable("dbo.Clues");
            DropTable("dbo.Categories");
            DropTable("dbo.Answers");
            DropTable("dbo.DoubleJeopardies");
            DropTable("dbo.GameModels");
            DropTable("dbo.BaseGameModels");
        }
    }
}
