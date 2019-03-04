using Microsoft.EntityFrameworkCore.Migrations;

namespace Foodie1.Data.Migrations
{
    public partial class IdCard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdCard",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdCard",
                table: "AspNetUsers");
        }
    }
}
