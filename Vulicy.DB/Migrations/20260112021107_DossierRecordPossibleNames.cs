using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class DossierRecordPossibleNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PossibleNamesBeNark",
                table: "DossierRecord",
                type: "json",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PossibleNamesRu",
                table: "DossierRecord",
                type: "json",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PossibleNamesBeNark",
                table: "DossierRecord");

            migrationBuilder.DropColumn(
                name: "PossibleNamesRu",
                table: "DossierRecord");
        }
    }
}
