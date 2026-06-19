using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAttackTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttackTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CritTypes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisarmMod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubdualMod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Signature = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttackTableRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttackTableId = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RollLow = table.Column<int>(type: "int", nullable: false),
                    RollHigh = table.Column<int>(type: "int", nullable: false),
                    At1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    At2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    At3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    At4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    At5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    At6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    At7 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    At8 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    At9 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    At10 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackTableRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttackTableRows_AttackTables_AttackTableId",
                        column: x => x.AttackTableId,
                        principalTable: "AttackTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttackTableWeapons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttackTableId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SizeMod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Length = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Strength = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fumble = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackTableWeapons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttackTableWeapons_AttackTables_AttackTableId",
                        column: x => x.AttackTableId,
                        principalTable: "AttackTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterFavoriteAttacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    AttackTableId = table.Column<int>(type: "int", nullable: false),
                    PreferredSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterFavoriteAttacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterFavoriteAttacks_AttackTables_AttackTableId",
                        column: x => x.AttackTableId,
                        principalTable: "AttackTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterFavoriteAttacks_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttackTableRows_AttackTableId_Size",
                table: "AttackTableRows",
                columns: new[] { "AttackTableId", "Size" });

            migrationBuilder.CreateIndex(
                name: "IX_AttackTables_Name",
                table: "AttackTables",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttackTableWeapons_AttackTableId",
                table: "AttackTableWeapons",
                column: "AttackTableId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterFavoriteAttacks_AttackTableId",
                table: "CharacterFavoriteAttacks",
                column: "AttackTableId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterFavoriteAttacks_CharacterId_AttackTableId",
                table: "CharacterFavoriteAttacks",
                columns: new[] { "CharacterId", "AttackTableId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttackTableRows");

            migrationBuilder.DropTable(
                name: "AttackTableWeapons");

            migrationBuilder.DropTable(
                name: "CharacterFavoriteAttacks");

            migrationBuilder.DropTable(
                name: "AttackTables");
        }
    }
}
