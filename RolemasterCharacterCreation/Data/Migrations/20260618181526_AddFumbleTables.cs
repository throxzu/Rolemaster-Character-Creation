using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFumbleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FumbleTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Signature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Col1Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col2Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col3Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col4Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col5Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FumbleTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FumbleTableRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FumbleTableId = table.Column<int>(type: "int", nullable: false),
                    RollLow = table.Column<int>(type: "int", nullable: false),
                    RollHigh = table.Column<int>(type: "int", nullable: false),
                    Col1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Col5 = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FumbleTableRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FumbleTableRows_FumbleTables_FumbleTableId",
                        column: x => x.FumbleTableId,
                        principalTable: "FumbleTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FumbleTableRows_FumbleTableId",
                table: "FumbleTableRows",
                column: "FumbleTableId");

            migrationBuilder.CreateIndex(
                name: "IX_FumbleTables_Name",
                table: "FumbleTables",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FumbleTableRows");

            migrationBuilder.DropTable(
                name: "FumbleTables");
        }
    }
}
