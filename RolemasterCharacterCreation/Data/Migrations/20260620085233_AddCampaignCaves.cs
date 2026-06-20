using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignCaves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinkedCaveId",
                table: "WorldLocations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CaveCategories",
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
                    table.PrimaryKey("PK_CaveCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaveMaps",
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
                    table.PrimaryKey("PK_CaveMaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaveCategoryNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaveCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaveCategoryNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaveCategoryNames_CaveCategories_CaveCategoryId",
                        column: x => x.CaveCategoryId,
                        principalTable: "CaveCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaveLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaveMapId = table.Column<int>(type: "int", nullable: false),
                    CellX = table.Column<int>(type: "int", nullable: false),
                    CellY = table.Column<int>(type: "int", nullable: false),
                    CaveCategoryId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GmNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisibleToPlayers = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaveLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaveLocations_CaveCategories_CaveCategoryId",
                        column: x => x.CaveCategoryId,
                        principalTable: "CaveCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CaveLocations_CaveMaps_CaveMapId",
                        column: x => x.CaveMapId,
                        principalTable: "CaveMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaveNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaveMapId = table.Column<int>(type: "int", nullable: false),
                    Ref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisibleToPlayers = table.Column<bool>(type: "bit", nullable: false),
                    X = table.Column<double>(type: "float", nullable: false),
                    Y = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaveNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaveNotes_CaveMaps_CaveMapId",
                        column: x => x.CaveMapId,
                        principalTable: "CaveMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaveReveals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaveMapId = table.Column<int>(type: "int", nullable: false),
                    CellX = table.Column<int>(type: "int", nullable: false),
                    CellY = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaveReveals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaveReveals_CaveMaps_CaveMapId",
                        column: x => x.CaveMapId,
                        principalTable: "CaveMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorldLocations_LinkedCaveId",
                table: "WorldLocations",
                column: "LinkedCaveId");

            migrationBuilder.CreateIndex(
                name: "IX_CaveCategoryNames_CaveCategoryId",
                table: "CaveCategoryNames",
                column: "CaveCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CaveLocations_CaveCategoryId",
                table: "CaveLocations",
                column: "CaveCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CaveLocations_CaveMapId_CellX_CellY",
                table: "CaveLocations",
                columns: new[] { "CaveMapId", "CellX", "CellY" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaveNotes_CaveMapId",
                table: "CaveNotes",
                column: "CaveMapId");

            migrationBuilder.CreateIndex(
                name: "IX_CaveReveals_CaveMapId_CellX_CellY",
                table: "CaveReveals",
                columns: new[] { "CaveMapId", "CellX", "CellY" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorldLocations_CaveMaps_LinkedCaveId",
                table: "WorldLocations",
                column: "LinkedCaveId",
                principalTable: "CaveMaps",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorldLocations_CaveMaps_LinkedCaveId",
                table: "WorldLocations");

            migrationBuilder.DropTable(
                name: "CaveCategoryNames");

            migrationBuilder.DropTable(
                name: "CaveLocations");

            migrationBuilder.DropTable(
                name: "CaveNotes");

            migrationBuilder.DropTable(
                name: "CaveReveals");

            migrationBuilder.DropTable(
                name: "CaveCategories");

            migrationBuilder.DropTable(
                name: "CaveMaps");

            migrationBuilder.DropIndex(
                name: "IX_WorldLocations_LinkedCaveId",
                table: "WorldLocations");

            migrationBuilder.DropColumn(
                name: "LinkedCaveId",
                table: "WorldLocations");
        }
    }
}
