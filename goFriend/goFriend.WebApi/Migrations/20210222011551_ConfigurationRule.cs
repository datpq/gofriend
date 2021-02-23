using Microsoft.EntityFrameworkCore.Migrations;

namespace goFriend.MobileAppService.Migrations
{
    public partial class ConfigurationRule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Configuration",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Rule",
                table: "Configuration",
                type: "NVARCHAR(500)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Configuration");

            migrationBuilder.DropColumn(
                name: "Rule",
                table: "Configuration");
        }
    }
}
