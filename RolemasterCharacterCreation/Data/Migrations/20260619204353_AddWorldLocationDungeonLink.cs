using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorldLocationDungeonLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinkedDungeonId",
                table: "WorldLocations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorldLocations_LinkedDungeonId",
                table: "WorldLocations",
                column: "LinkedDungeonId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorldLocations_DungeonMaps_LinkedDungeonId",
                table: "WorldLocations",
                column: "LinkedDungeonId",
                principalTable: "DungeonMaps",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorldLocations_DungeonMaps_LinkedDungeonId",
                table: "WorldLocations");

            migrationBuilder.DropIndex(
                name: "IX_WorldLocations_LinkedDungeonId",
                table: "WorldLocations");

            migrationBuilder.DropColumn(
                name: "LinkedDungeonId",
                table: "WorldLocations");
        }
    }
}
