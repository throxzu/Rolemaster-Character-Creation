using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddItemCombatBonuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliesTo",
                table: "CharacterEquipmentItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DbBonus",
                table: "CharacterEquipmentItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsEquipped",
                table: "CharacterEquipmentItems",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "ObBonus",
                table: "CharacterEquipmentItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppliesTo",
                table: "CharacterEquipmentItems");

            migrationBuilder.DropColumn(
                name: "DbBonus",
                table: "CharacterEquipmentItems");

            migrationBuilder.DropColumn(
                name: "IsEquipped",
                table: "CharacterEquipmentItems");

            migrationBuilder.DropColumn(
                name: "ObBonus",
                table: "CharacterEquipmentItems");
        }
    }
}
