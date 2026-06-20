using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkTownVillageToBuilding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinkedBuildingId",
                table: "VillageLocations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LinkedBuildingId",
                table: "TownLocations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VillageLocations_LinkedBuildingId",
                table: "VillageLocations",
                column: "LinkedBuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_TownLocations_LinkedBuildingId",
                table: "TownLocations",
                column: "LinkedBuildingId");

            migrationBuilder.AddForeignKey(
                name: "FK_TownLocations_BuildingMaps_LinkedBuildingId",
                table: "TownLocations",
                column: "LinkedBuildingId",
                principalTable: "BuildingMaps",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VillageLocations_BuildingMaps_LinkedBuildingId",
                table: "VillageLocations",
                column: "LinkedBuildingId",
                principalTable: "BuildingMaps",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TownLocations_BuildingMaps_LinkedBuildingId",
                table: "TownLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_VillageLocations_BuildingMaps_LinkedBuildingId",
                table: "VillageLocations");

            migrationBuilder.DropIndex(
                name: "IX_VillageLocations_LinkedBuildingId",
                table: "VillageLocations");

            migrationBuilder.DropIndex(
                name: "IX_TownLocations_LinkedBuildingId",
                table: "TownLocations");

            migrationBuilder.DropColumn(
                name: "LinkedBuildingId",
                table: "VillageLocations");

            migrationBuilder.DropColumn(
                name: "LinkedBuildingId",
                table: "TownLocations");
        }
    }
}
