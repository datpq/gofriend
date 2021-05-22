using Microsoft.EntityFrameworkCore.Migrations;

namespace goFriend.MobileAppService.Migrations
{
    public partial class GoFamily : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FriendLocations",
                table: "FriendLocations");

            migrationBuilder.AddColumn<bool>(
                name: "Public",
                table: "Groups",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "FriendLocations",
                type: "VARCHAR(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FriendLocations",
                table: "FriendLocations",
                columns: new[] { "FriendId", "DeviceId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FriendLocations",
                table: "FriendLocations");

            migrationBuilder.DropColumn(
                name: "Public",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "FriendLocations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FriendLocations",
                table: "FriendLocations",
                column: "FriendId");
        }
    }
}
