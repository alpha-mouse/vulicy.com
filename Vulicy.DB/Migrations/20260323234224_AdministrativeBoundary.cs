using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class AdministrativeBoundary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Geometry>(
                name: "Boundary",
                table: "Administrative",
                type: "geometry(Geometry, 4326)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Boundary",
                table: "Administrative");
        }
    }
}
