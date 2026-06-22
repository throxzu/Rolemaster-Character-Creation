using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    PublicNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GmNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionNotes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionNotes_SessionDate",
                table: "SessionNotes",
                column: "SessionDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionNotes");
        }
    }
}
