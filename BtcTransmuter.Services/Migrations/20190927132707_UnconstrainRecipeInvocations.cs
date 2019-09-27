using Microsoft.EntityFrameworkCore.Migrations;

namespace BtcTransmuter.Data.Migrations
{
    public partial class UnconstrainRecipeInvocations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeInvocations_RecipeActions_RecipeActionId",
                table: "RecipeInvocations");

            migrationBuilder.DropIndex(
                name: "IX_RecipeInvocations_RecipeActionId",
                table: "RecipeInvocations");

            migrationBuilder.RenameColumn(
                name: "RecipeActionId",
                table: "RecipeInvocations",
                newName: "RecipeAction");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecipeAction",
                table: "RecipeInvocations",
                newName: "RecipeActionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeInvocations_RecipeActionId",
                table: "RecipeInvocations",
                column: "RecipeActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeInvocations_RecipeActions_RecipeActionId",
                table: "RecipeInvocations",
                column: "RecipeActionId",
                principalTable: "RecipeActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
