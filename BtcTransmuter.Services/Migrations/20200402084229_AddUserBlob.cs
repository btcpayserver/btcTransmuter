using Microsoft.EntityFrameworkCore.Migrations;

namespace BtcTransmuter.Data.Migrations
{
    public partial class AddUserBlob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataJson",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataJson",
                table: "AspNetUsers");
        }
    }
}
