using Microsoft.EntityFrameworkCore.Migrations;

namespace BtcTransmuter.Data.Migrations
{
    public partial class FixCascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeInvocations_RecipeActions_RecipeActionId",
                table: "RecipeInvocations");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeInvocations_RecipeActions_RecipeActionId",
                table: "RecipeInvocations",
                column: "RecipeActionId",
                principalTable: "RecipeActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeInvocations_RecipeActions_RecipeActionId",
                table: "RecipeInvocations");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeInvocations_RecipeActions_RecipeActionId",
                table: "RecipeInvocations",
                column: "RecipeActionId",
                principalTable: "RecipeActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
