using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignDungeons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DungeonCategories",
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
                    table.PrimaryKey("PK_DungeonCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DungeonMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DungeonMaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DungeonCategoryNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DungeonCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DungeonCategoryNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DungeonCategoryNames_DungeonCategories_DungeonCategoryId",
                        column: x => x.DungeonCategoryId,
                        principalTable: "DungeonCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DungeonLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DungeonMapId = table.Column<int>(type: "int", nullable: false),
                    RectIndex = table.Column<int>(type: "int", nullable: false),
                    DungeonCategoryId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GmNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DungeonLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DungeonLocations_DungeonCategories_DungeonCategoryId",
                        column: x => x.DungeonCategoryId,
                        principalTable: "DungeonCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DungeonLocations_DungeonMaps_DungeonMapId",
                        column: x => x.DungeonMapId,
                        principalTable: "DungeonMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DungeonReveals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DungeonMapId = table.Column<int>(type: "int", nullable: false),
                    RectIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DungeonReveals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DungeonReveals_DungeonMaps_DungeonMapId",
                        column: x => x.DungeonMapId,
                        principalTable: "DungeonMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DungeonCategoryNames_DungeonCategoryId",
                table: "DungeonCategoryNames",
                column: "DungeonCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DungeonLocations_DungeonCategoryId",
                table: "DungeonLocations",
                column: "DungeonCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DungeonLocations_DungeonMapId_RectIndex",
                table: "DungeonLocations",
                columns: new[] { "DungeonMapId", "RectIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DungeonReveals_DungeonMapId_RectIndex",
                table: "DungeonReveals",
                columns: new[] { "DungeonMapId", "RectIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DungeonCategoryNames");

            migrationBuilder.DropTable(
                name: "DungeonLocations");

            migrationBuilder.DropTable(
                name: "DungeonReveals");

            migrationBuilder.DropTable(
                name: "DungeonCategories");

            migrationBuilder.DropTable(
                name: "DungeonMaps");
        }
    }
}
