using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameShieldTypesToRmu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Data only: shields took the book's own names (Table 9-6) in place of the
            // invented Small/Medium/Large, and gained a base DB that goes with them.
            migrationBuilder.Sql(@"
                UPDATE [Characters] SET [ShieldType] =
                    CASE [ShieldType] WHEN 'Small'  THEN 'Target'
                                      WHEN 'Medium' THEN 'Normal'
                                      WHEN 'Large'  THEN 'Full' END
                WHERE [ShieldType] IN ('Small', 'Medium', 'Large');");

            // The wizard used to store the literal 'None' where the armour editor stores NULL.
            migrationBuilder.Sql(
                "UPDATE [Characters] SET [ShieldType] = NULL WHERE [ShieldType] = 'None';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 'Wall' collapses to 'Large' — the old vocabulary has nowhere else to put it.
            migrationBuilder.Sql(@"
                UPDATE [Characters] SET [ShieldType] =
                    CASE [ShieldType] WHEN 'Target' THEN 'Small'
                                      WHEN 'Normal' THEN 'Medium'
                                      WHEN 'Full'   THEN 'Large'
                                      WHEN 'Wall'   THEN 'Large' END
                WHERE [ShieldType] IN ('Target', 'Normal', 'Full', 'Wall');");
        }
    }
}
