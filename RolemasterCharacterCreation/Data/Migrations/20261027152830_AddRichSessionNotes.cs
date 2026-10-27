using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRichSessionNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotesAreHtml",
                table: "SessionNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SessionNoteImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionNoteId = table.Column<int>(type: "int", nullable: true),
                    GmOnly = table.Column<bool>(type: "bit", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    OriginalContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionNoteImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionNoteImages_SessionNotes_SessionNoteId",
                        column: x => x.SessionNoteId,
                        principalTable: "SessionNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionNoteImages_SessionNoteId",
                table: "SessionNoteImages",
                column: "SessionNoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionNoteImages");

            migrationBuilder.DropColumn(
                name: "NotesAreHtml",
                table: "SessionNotes");
        }
    }
}
