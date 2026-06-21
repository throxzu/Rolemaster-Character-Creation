using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RolemasterCharacterCreation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LoginUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    InviteSmsTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignSettings");
        }
    }
}
