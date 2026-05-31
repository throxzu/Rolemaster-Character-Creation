using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArmorSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ArmorFullSuit",
                table: "Characters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ArmorType",
                table: "Characters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GreavesGrade",
                table: "Characters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HelmetGrade",
                table: "Characters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShieldType",
                table: "Characters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VambracesGrade",
                table: "Characters",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArmorFullSuit",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ArmorType",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "GreavesGrade",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HelmetGrade",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ShieldType",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "VambracesGrade",
                table: "Characters");
        }
    }
}
