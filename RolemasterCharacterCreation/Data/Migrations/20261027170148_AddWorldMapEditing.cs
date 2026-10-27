using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorldMapEditing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalJson",
                table: "WorldMaps",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalJson",
                table: "WorldMaps");
        }
    }
}
