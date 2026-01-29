using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class DropFeatureIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FeatureHistoric");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Feature");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FeatureHistoric",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Feature",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
