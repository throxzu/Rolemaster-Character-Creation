using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeduplicateCharacterTalents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nothing stopped the same talent being added twice, so characters accumulated
            // duplicates across level-ups and were charged DP for each copy. Talents are
            // tiered rather than repeatable, so collapse each group to its highest tier.
            //
            // Grouped by restriction as well as name: a talent taken for a specialization is
            // genuinely separate, so Magical Resistance (Fire) and (Cold) must both survive.
            migrationBuilder.Sql(@"
                WITH ranked AS (
                    SELECT [Id],
                           ROW_NUMBER() OVER (
                               PARTITION BY [CharacterId], [TalentName], ISNULL([Restriction], N'')
                               ORDER BY [Tier] DESC, [Id] ASC) AS rn
                    FROM [CharacterTalents])
                DELETE FROM [CharacterTalents]
                WHERE [Id] IN (SELECT [Id] FROM ranked WHERE rn > 1);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The discarded rows are gone; duplicates cannot be put back, and re-creating
            // them would be inventing data rather than restoring it.
        }
    }
}
