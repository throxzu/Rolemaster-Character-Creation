using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorldReveals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorldReveals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorldMapId = table.Column<int>(type: "int", nullable: false),
                    HexQ = table.Column<int>(type: "int", nullable: false),
                    HexR = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldReveals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorldReveals_WorldMaps_WorldMapId",
                        column: x => x.WorldMapId,
                        principalTable: "WorldMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorldReveals_WorldMapId_HexQ_HexR",
                table: "WorldReveals",
                columns: new[] { "WorldMapId", "HexQ", "HexR" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorldReveals");
        }
    }
}
