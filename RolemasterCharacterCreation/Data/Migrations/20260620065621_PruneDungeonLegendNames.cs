using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class PruneDungeonLegendNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only the hidden legends keep a preset name list; remove the names from the rest.
            migrationBuilder.Sql(@"
                DELETE n FROM [DungeonCategoryNames] n
                INNER JOIN [DungeonCategories] c ON n.[DungeonCategoryId] = c.[Id]
                WHERE c.[Name] NOT IN ('Trap', 'Secret Door / Passage', 'Hidden Cache');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
