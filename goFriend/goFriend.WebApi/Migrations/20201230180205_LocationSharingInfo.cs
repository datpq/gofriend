using Microsoft.EntityFrameworkCore.Migrations;

namespace goFriend.MobileAppService.Migrations
{
    public partial class LocationSharingInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SharingInfo",
                table: "FriendLocations",
                type: "VARCHAR(400)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharingInfo",
                table: "FriendLocations");
        }
    }
}
