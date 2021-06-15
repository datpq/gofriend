using Microsoft.EntityFrameworkCore.Migrations;

namespace goFriend.MobileAppService.Migrations
{
    public partial class RemoveGroupNameUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Groups_Name",
                table: "Groups");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Groups_Name",
                table: "Groups",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");
        }
    }
}
