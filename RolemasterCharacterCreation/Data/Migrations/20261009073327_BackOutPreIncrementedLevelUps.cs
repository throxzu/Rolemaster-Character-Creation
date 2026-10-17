using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackOutPreIncrementedLevelUps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Data only: the level used to be awarded when Level Up was clicked, and the wizard
            // read it back as "a level-up is under way". It is now awarded when the wizard
            // finishes, so anyone caught part-way through at deploy time is carrying a level
            // they have not finished earning and would be given a second one at Finish.
            // The frozen baseline is what marks a level-up in flight.
            migrationBuilder.Sql(@"
                UPDATE [Characters] SET [Level] = [Level] - 1
                WHERE [LevelUpBaselineJson] IS NOT NULL AND [Level] > 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [Characters] SET [Level] = [Level] + 1
                WHERE [LevelUpBaselineJson] IS NOT NULL;");
        }
    }
}
