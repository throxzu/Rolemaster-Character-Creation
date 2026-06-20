using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorldLocationLinkedTown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinkedTownId",
                table: "WorldLocations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorldLocations_LinkedTownId",
                table: "WorldLocations",
                column: "LinkedTownId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorldLocations_Towns_LinkedTownId",
                table: "WorldLocations",
                column: "LinkedTownId",
                principalTable: "Towns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorldLocations_Towns_LinkedTownId",
                table: "WorldLocations");

            migrationBuilder.DropIndex(
                name: "IX_WorldLocations_LinkedTownId",
                table: "WorldLocations");

            migrationBuilder.DropColumn(
                name: "LinkedTownId",
                table: "WorldLocations");
        }
    }
}
