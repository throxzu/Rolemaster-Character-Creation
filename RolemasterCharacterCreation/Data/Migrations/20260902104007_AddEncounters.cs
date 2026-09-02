using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEncounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Encounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsTemplate = table.Column<bool>(type: "bit", nullable: false),
                    Round = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encounters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EncounterCombatants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncounterId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    MobNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    CharacterId = table.Column<int>(type: "int", nullable: true),
                    BoardX = table.Column<double>(type: "float", nullable: false),
                    BoardY = table.Column<double>(type: "float", nullable: false),
                    MaxHits = table.Column<int>(type: "int", nullable: false),
                    CurrentHits = table.Column<int>(type: "int", nullable: false),
                    StunRounds = table.Column<int>(type: "int", nullable: false),
                    BleedPerRound = table.Column<int>(type: "int", nullable: false),
                    DiesInRounds = table.Column<int>(type: "int", nullable: false),
                    Prone = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncounterCombatants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncounterCombatants_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EncounterCombatants_Encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "Encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EncounterCombatants_CharacterId",
                table: "EncounterCombatants",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_EncounterCombatants_EncounterId",
                table: "EncounterCombatants",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_IsTemplate_LastUsedAt",
                table: "Encounters",
                columns: new[] { "IsTemplate", "LastUsedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EncounterCombatants");

            migrationBuilder.DropTable(
                name: "Encounters");
        }
    }
}
