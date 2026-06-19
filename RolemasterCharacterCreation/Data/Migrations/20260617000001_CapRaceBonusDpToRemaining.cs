using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class CapRaceBonusDpToRemaining : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RaceBonusDp now represents the *remaining* racial bonus pool, not the original total.
            // Existing characters have already consumed up to 25 bonus DP per completed level.
            migrationBuilder.Sql("""
                UPDATE Characters
                SET RaceBonusDp = CASE WHEN RaceBonusDp - (Level - 1) * 25 < 0 THEN 0
                                       ELSE RaceBonusDp - (Level - 1) * 25 END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Cannot recover the original pool value without storing it separately.
        }
    }
}
