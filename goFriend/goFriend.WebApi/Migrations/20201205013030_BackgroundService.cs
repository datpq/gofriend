using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace goFriend.MobileAppService.Migrations
{
    public partial class BackgroundService : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessage_Chat_ChatId",
                table: "ChatMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessage_Friends_OwnerId",
                table: "ChatMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupFixedCatValues_Groups_GroupId",
                table: "GroupFixedCatValues");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupFriends_Friends_FriendId",
                table: "GroupFriends");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupFriends_Groups_GroupId",
                table: "GroupFriends");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupPredefinedCategory_Groups_GroupId",
                table: "GroupPredefinedCategory");

            migrationBuilder.AddColumn<int>(
                name: "ServiceSleepTimeout",
                table: "Settings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FriendLocations",
                columns: table => new
                {
                    FriendId = table.Column<int>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Location = table.Column<Point>(type: "GEOMETRY", nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendLocations", x => x.FriendId);
                    table.ForeignKey(
                        name: "FK_FriendLocations_Friends_FriendId",
                        column: x => x.FriendId,
                        principalTable: "Friends",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessage_Chat_ChatId",
                table: "ChatMessage",
                column: "ChatId",
                principalTable: "Chat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessage_Friends_OwnerId",
                table: "ChatMessage",
                column: "OwnerId",
                principalTable: "Friends",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupFixedCatValues_Groups_GroupId",
                table: "GroupFixedCatValues",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupFriends_Friends_FriendId",
                table: "GroupFriends",
                column: "FriendId",
                principalTable: "Friends",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupFriends_Groups_GroupId",
                table: "GroupFriends",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPredefinedCategory_Groups_GroupId",
                table: "GroupPredefinedCategory",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessage_Chat_ChatId",
                table: "ChatMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessage_Friends_OwnerId",
                table: "ChatMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupFixedCatValues_Groups_GroupId",
                table: "GroupFixedCatValues");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupFriends_Friends_FriendId",
                table: "GroupFriends");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupFriends_Groups_GroupId",
                table: "GroupFriends");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupPredefinedCategory_Groups_GroupId",
                table: "GroupPredefinedCategory");

            migrationBuilder.DropTable(
                name: "FriendLocations");

            migrationBuilder.DropColumn(
                name: "ServiceSleepTimeout",
                table: "Settings");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessage_Chat_ChatId",
                table: "ChatMessage",
                column: "ChatId",
                principalTable: "Chat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessage_Friends_OwnerId",
                table: "ChatMessage",
                column: "OwnerId",
                principalTable: "Friends",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupFixedCatValues_Groups_GroupId",
                table: "GroupFixedCatValues",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupFriends_Friends_FriendId",
                table: "GroupFriends",
                column: "FriendId",
                principalTable: "Friends",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupFriends_Groups_GroupId",
                table: "GroupFriends",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPredefinedCategory_Groups_GroupId",
                table: "GroupPredefinedCategory",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
