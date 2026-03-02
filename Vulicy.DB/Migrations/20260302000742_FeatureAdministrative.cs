using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class FeatureAdministrative : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdministrativeId",
                table: "FeatureHistoric",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdministrativeId",
                table: "Feature",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feature_AdministrativeId",
                table: "Feature",
                column: "AdministrativeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feature_Administrative_AdministrativeId",
                table: "Feature",
                column: "AdministrativeId",
                principalTable: "Administrative",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feature_Administrative_AdministrativeId",
                table: "Feature");

            migrationBuilder.DropIndex(
                name: "IX_Feature_AdministrativeId",
                table: "Feature");

            migrationBuilder.DropColumn(
                name: "AdministrativeId",
                table: "FeatureHistoric");

            migrationBuilder.DropColumn(
                name: "AdministrativeId",
                table: "Feature");
        }
    }
}
