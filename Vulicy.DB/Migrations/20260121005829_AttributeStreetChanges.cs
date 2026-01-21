using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class AttributeStreetChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeatureHistoric_DossierRecord_DossierRecordId",
                table: "FeatureHistoric");

            migrationBuilder.DropForeignKey(
                name: "FK_FeatureHistoric_NamingCategory_NamingCategoryId",
                table: "FeatureHistoric");

            migrationBuilder.DropIndex(
                name: "IX_FeatureHistoric_DossierRecordId",
                table: "FeatureHistoric");

            migrationBuilder.DropIndex(
                name: "IX_FeatureHistoric_NamingCategoryId",
                table: "FeatureHistoric");

            migrationBuilder.AddColumn<int>(
                name: "InHistoryById",
                table: "FeatureHistoric",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedById",
                table: "FeatureHistoric",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedById",
                table: "Feature",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feature_LastModifiedById",
                table: "Feature",
                column: "LastModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Feature_User_LastModifiedById",
                table: "Feature",
                column: "LastModifiedById",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.Sql("""
                INSERT INTO "User" ("Id", "ExternalId", "Username", "Email", "IsAdmin", "CreatedDateTime", "ModifiedDateTime")
                VALUES (0, 0, 'System', 'system@vulicy.com', false, '2026-01-01', '2026-01-01');

                SELECT setval(pg_get_serial_sequence('"User"', 'Id'), (SELECT MAX("Id") FROM "User"));
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""User"" WHERE ""Id"" = 0;");

            migrationBuilder.DropForeignKey(
                name: "FK_Feature_User_LastModifiedById",
                table: "Feature");

            migrationBuilder.DropIndex(
                name: "IX_Feature_LastModifiedById",
                table: "Feature");

            migrationBuilder.DropColumn(
                name: "InHistoryById",
                table: "FeatureHistoric");

            migrationBuilder.DropColumn(
                name: "LastModifiedById",
                table: "FeatureHistoric");

            migrationBuilder.DropColumn(
                name: "LastModifiedById",
                table: "Feature");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureHistoric_DossierRecordId",
                table: "FeatureHistoric",
                column: "DossierRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureHistoric_NamingCategoryId",
                table: "FeatureHistoric",
                column: "NamingCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureHistoric_DossierRecord_DossierRecordId",
                table: "FeatureHistoric",
                column: "DossierRecordId",
                principalTable: "DossierRecord",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureHistoric_NamingCategory_NamingCategoryId",
                table: "FeatureHistoric",
                column: "NamingCategoryId",
                principalTable: "NamingCategory",
                principalColumn: "Id");
        }
    }
}
