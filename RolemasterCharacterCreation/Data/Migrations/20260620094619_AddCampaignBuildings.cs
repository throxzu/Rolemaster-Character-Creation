using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignBuildings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinkedBuildingId",
                table: "WorldLocations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BuildingCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsBuiltIn = table.Column<bool>(type: "bit", nullable: false),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BuildingMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawSvg = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingMaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BuildingCategoryNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingCategoryNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingCategoryNames_BuildingCategories_BuildingCategoryId",
                        column: x => x.BuildingCategoryId,
                        principalTable: "BuildingCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingMapId = table.Column<int>(type: "int", nullable: false),
                    FloorIndex = table.Column<int>(type: "int", nullable: false),
                    CellX = table.Column<int>(type: "int", nullable: false),
                    CellY = table.Column<int>(type: "int", nullable: false),
                    BuildingCategoryId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GmNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisibleToPlayers = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingLocations_BuildingCategories_BuildingCategoryId",
                        column: x => x.BuildingCategoryId,
                        principalTable: "BuildingCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BuildingLocations_BuildingMaps_BuildingMapId",
                        column: x => x.BuildingMapId,
                        principalTable: "BuildingMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingMapId = table.Column<int>(type: "int", nullable: false),
                    FloorIndex = table.Column<int>(type: "int", nullable: false),
                    Ref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisibleToPlayers = table.Column<bool>(type: "bit", nullable: false),
                    X = table.Column<double>(type: "float", nullable: false),
                    Y = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingNotes_BuildingMaps_BuildingMapId",
                        column: x => x.BuildingMapId,
                        principalTable: "BuildingMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingReveals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingMapId = table.Column<int>(type: "int", nullable: false),
                    FloorIndex = table.Column<int>(type: "int", nullable: false),
                    CellX = table.Column<int>(type: "int", nullable: false),
                    CellY = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingReveals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingReveals_BuildingMaps_BuildingMapId",
                        column: x => x.BuildingMapId,
                        principalTable: "BuildingMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorldLocations_LinkedBuildingId",
                table: "WorldLocations",
                column: "LinkedBuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingCategoryNames_BuildingCategoryId",
                table: "BuildingCategoryNames",
                column: "BuildingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingLocations_BuildingCategoryId",
                table: "BuildingLocations",
                column: "BuildingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingLocations_BuildingMapId_FloorIndex_CellX_CellY",
                table: "BuildingLocations",
                columns: new[] { "BuildingMapId", "FloorIndex", "CellX", "CellY" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingNotes_BuildingMapId",
                table: "BuildingNotes",
                column: "BuildingMapId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingReveals_BuildingMapId_FloorIndex_CellX_CellY",
                table: "BuildingReveals",
                columns: new[] { "BuildingMapId", "FloorIndex", "CellX", "CellY" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorldLocations_BuildingMaps_LinkedBuildingId",
                table: "WorldLocations",
                column: "LinkedBuildingId",
                principalTable: "BuildingMaps",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorldLocations_BuildingMaps_LinkedBuildingId",
                table: "WorldLocations");

            migrationBuilder.DropTable(
                name: "BuildingCategoryNames");

            migrationBuilder.DropTable(
                name: "BuildingLocations");

            migrationBuilder.DropTable(
                name: "BuildingNotes");

            migrationBuilder.DropTable(
                name: "BuildingReveals");

            migrationBuilder.DropTable(
                name: "BuildingCategories");

            migrationBuilder.DropTable(
                name: "BuildingMaps");

            migrationBuilder.DropIndex(
                name: "IX_WorldLocations_LinkedBuildingId",
                table: "WorldLocations");

            migrationBuilder.DropColumn(
                name: "LinkedBuildingId",
                table: "WorldLocations");
        }
    }
}
