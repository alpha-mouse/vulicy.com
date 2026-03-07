using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class DossierRecordForumLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ForumRelativeLink",
                table: "DossierRecordHistoric",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ForumRelativeLink",
                table: "DossierRecord",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForumRelativeLink",
                table: "DossierRecordHistoric");

            migrationBuilder.DropColumn(
                name: "ForumRelativeLink",
                table: "DossierRecord");
        }
    }
}
