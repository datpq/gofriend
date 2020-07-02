using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace goFriend.MobileAppService.Migrations
{
    public partial class ChatOneToOne : Migration
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

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Chat",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Chat",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OwnerId",
                table: "Chat",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Friends_OwnerId",
                table: "Chat",
                column: "OwnerId",
                principalTable: "Friends",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Friends_OwnerId",
                table: "Chat");

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

            migrationBuilder.DropIndex(
                name: "IX_Chat_OwnerId",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chat");

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
    }
}
