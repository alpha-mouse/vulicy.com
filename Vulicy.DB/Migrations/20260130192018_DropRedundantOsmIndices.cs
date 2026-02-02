using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class DropRedundantOsmIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OsmFeatureImport_Geometry",
                table: "OsmFeatureImport");

            migrationBuilder.DropIndex(
                name: "IX_OsmFeatureHistoric_Geometry",
                table: "OsmFeatureHistoric");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OsmFeatureImport_Geometry",
                table: "OsmFeatureImport",
                column: "Geometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_OsmFeatureHistoric_Geometry",
                table: "OsmFeatureHistoric",
                column: "Geometry")
                .Annotation("Npgsql:IndexMethod", "gist");
        }
    }
}
