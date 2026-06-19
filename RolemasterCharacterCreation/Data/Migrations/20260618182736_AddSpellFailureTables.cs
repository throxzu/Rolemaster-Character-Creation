using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpellFailureTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpellFailureTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Signature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Col1Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col2Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col3Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col4Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpellFailureTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpellFailureTableRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpellFailureTableId = table.Column<int>(type: "int", nullable: false),
                    RollLow = table.Column<int>(type: "int", nullable: false),
                    RollHigh = table.Column<int>(type: "int", nullable: false),
                    Col1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col4 = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpellFailureTableRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpellFailureTableRows_SpellFailureTables_SpellFailureTableId",
                        column: x => x.SpellFailureTableId,
                        principalTable: "SpellFailureTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpellFailureTableRows_SpellFailureTableId",
                table: "SpellFailureTableRows",
                column: "SpellFailureTableId");

            migrationBuilder.CreateIndex(
                name: "IX_SpellFailureTables_Name",
                table: "SpellFailureTables",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpellFailureTableRows");

            migrationBuilder.DropTable(
                name: "SpellFailureTables");
        }
    }
}
