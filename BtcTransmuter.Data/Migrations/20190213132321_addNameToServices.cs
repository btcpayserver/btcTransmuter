using Microsoft.EntityFrameworkCore.Migrations;

namespace BtcTransmuter.Data.Migrations
{
    public partial class addNameToServices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ExternalServices",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "ExternalServices");
        }
    }
}
