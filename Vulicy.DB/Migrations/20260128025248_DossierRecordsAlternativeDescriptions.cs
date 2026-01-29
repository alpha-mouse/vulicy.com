using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class DossierRecordsAlternativeDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternativeDescriptionsBe",
                table: "DossierRecordHistoric",
                type: "json",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternativeDescriptionsRu",
                table: "DossierRecordHistoric",
                type: "json",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternativeDescriptionsBe",
                table: "DossierRecord",
                type: "json",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternativeDescriptionsRu",
                table: "DossierRecord",
                type: "json",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternativeDescriptionsBe",
                table: "DossierRecordHistoric");

            migrationBuilder.DropColumn(
                name: "AlternativeDescriptionsRu",
                table: "DossierRecordHistoric");

            migrationBuilder.DropColumn(
                name: "AlternativeDescriptionsBe",
                table: "DossierRecord");

            migrationBuilder.DropColumn(
                name: "AlternativeDescriptionsRu",
                table: "DossierRecord");
        }
    }
}
