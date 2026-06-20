using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class DungeonMarksPerCell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Old marks colored a whole room; they have no square to map to, so clear them.
            migrationBuilder.Sql("DELETE FROM [DungeonLocations];");

            migrationBuilder.DropIndex(
                name: "IX_DungeonLocations_DungeonMapId_RectIndex",
                table: "DungeonLocations");

            migrationBuilder.RenameColumn(
                name: "RectIndex",
                table: "DungeonLocations",
                newName: "CellY");

            migrationBuilder.AddColumn<int>(
                name: "CellX",
                table: "DungeonLocations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DungeonLocations_DungeonMapId_CellX_CellY",
                table: "DungeonLocations",
                columns: new[] { "DungeonMapId", "CellX", "CellY" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DungeonLocations_DungeonMapId_CellX_CellY",
                table: "DungeonLocations");

            migrationBuilder.DropColumn(
                name: "CellX",
                table: "DungeonLocations");

            migrationBuilder.RenameColumn(
                name: "CellY",
                table: "DungeonLocations",
                newName: "RectIndex");

            migrationBuilder.CreateIndex(
                name: "IX_DungeonLocations_DungeonMapId_RectIndex",
                table: "DungeonLocations",
                columns: new[] { "DungeonMapId", "RectIndex" },
                unique: true);
        }
    }
}
