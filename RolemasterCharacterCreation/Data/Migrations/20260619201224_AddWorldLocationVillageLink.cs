using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorldLocationVillageLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinkedVillageId",
                table: "WorldLocations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorldLocations_LinkedVillageId",
                table: "WorldLocations",
                column: "LinkedVillageId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorldLocations_Villages_LinkedVillageId",
                table: "WorldLocations",
                column: "LinkedVillageId",
                principalTable: "Villages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorldLocations_Villages_LinkedVillageId",
                table: "WorldLocations");

            migrationBuilder.DropIndex(
                name: "IX_WorldLocations_LinkedVillageId",
                table: "WorldLocations");

            migrationBuilder.DropColumn(
                name: "LinkedVillageId",
                table: "WorldLocations");
        }
    }
}
