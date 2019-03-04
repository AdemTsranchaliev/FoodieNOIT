using Microsoft.EntityFrameworkCore.Migrations;

namespace Foodie1.Data.Migrations
{
    public partial class OrderProductAddReadyAndDelivery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Delivered",
                table: "OrderProducts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ready",
                table: "OrderProducts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Delivered",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "Ready",
                table: "OrderProducts");
        }
    }
}
