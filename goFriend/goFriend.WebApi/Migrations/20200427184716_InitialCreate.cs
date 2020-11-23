using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace goFriend.MobileAppService.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            return;
            migrationBuilder.CreateTable(
                name: "CacheConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeyPrefix = table.Column<string>(type: "VARCHAR(250)", nullable: true),
                    KeySuffixReg = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    Timeout = table.Column<int>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CacheConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "NVARCHAR(100)", nullable: true),
                    FirstName = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    LastName = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    MiddleName = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    FacebookId = table.Column<string>(type: "VARCHAR(20)", nullable: true),
                    Email = table.Column<string>(type: "VARCHAR(50)", nullable: true),
                    Birthday = table.Column<DateTime>(nullable: true),
                    Gender = table.Column<string>(type: "VARCHAR(10)", nullable: true),
                    Active = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    Location = table.Column<Point>(type: "GEOMETRY", nullable: true),
                    Address = table.Column<string>(type: "NVARCHAR(100)", nullable: true),
                    CountryName = table.Column<string>(type: "NVARCHAR(30)", nullable: true),
                    DeviceInfo = table.Column<string>(type: "VARCHAR(170)", nullable: true),
                    Info = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    Image = table.Column<byte[]>(nullable: true),
                    FacebookToken = table.Column<string>(type: "VARCHAR(400)", nullable: true),
                    ThirdPartyLogin = table.Column<string>(type: "VARCHAR(10)", nullable: false),
                    ThirdPartyToken = table.Column<string>(type: "VARCHAR(400)", nullable: true),
                    ThirdPartyUserId = table.Column<string>(type: "VARCHAR(50)", nullable: true),
                    Token = table.Column<Guid>(nullable: false),
                    ShowLocation = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Desc = table.Column<string>(type: "NVARCHAR(255)", nullable: true),
                    Info = table.Column<string>(type: "NVARCHAR(2000)", nullable: true),
                    Cat1Desc = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat2Desc = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat3Desc = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat4Desc = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat5Desc = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat6Desc = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat7Desc = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat8Desc = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat9Desc = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Active = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    Logo = table.Column<byte[]>(nullable: true),
                    LogoUrl = table.Column<string>(type: "VARCHAR(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(nullable: false),
                    NotificationJson = table.Column<string>(type: "NVARCHAR(1000)", nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    Destination = table.Column<string>(type: "NVARCHAR(200)", nullable: true),
                    Reads = table.Column<string>(type: "NVARCHAR(200)", nullable: true),
                    Deletions = table.Column<string>(type: "NVARCHAR(200)", nullable: true),
                    OwnerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Order = table.Column<int>(nullable: false),
                    Rule = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    LocationSwitch = table.Column<bool>(nullable: false),
                    DefaultShowLocation = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupFixedCatValues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(nullable: false),
                    Cat1 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat2 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat3 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat4 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat5 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat6 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat7 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat8 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat9 = table.Column<string>(type: "NVARCHAR(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupFixedCatValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupFixedCatValues_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupFriends",
                columns: table => new
                {
                    FriendId = table.Column<int>(nullable: false),
                    GroupId = table.Column<int>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Active = table.Column<bool>(nullable: false),
                    UserRight = table.Column<int>(nullable: false),
                    Cat1 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat2 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat3 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat4 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat5 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat6 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat7 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat8 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Cat9 = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupFriends", x => new { x.FriendId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_GroupFriends_Friends_FriendId",
                        column: x => x.FriendId,
                        principalTable: "Friends",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupFriends_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupPredefinedCategory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(nullable: false),
                    Category = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    ParentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPredefinedCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupPredefinedCategory_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPredefinedCategory_GroupPredefinedCategory_ParentId",
                        column: x => x.ParentId,
                        principalTable: "GroupPredefinedCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupFixedCatValues_GroupId",
                table: "GroupFixedCatValues",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupFriends_GroupId",
                table: "GroupFriends",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPredefinedCategory_GroupId",
                table: "GroupPredefinedCategory",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPredefinedCategory_ParentId",
                table: "GroupPredefinedCategory",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Name",
                table: "Groups",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CacheConfiguration");

            migrationBuilder.DropTable(
                name: "GroupFixedCatValues");

            migrationBuilder.DropTable(
                name: "GroupFriends");

            migrationBuilder.DropTable(
                name: "GroupPredefinedCategory");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Friends");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
