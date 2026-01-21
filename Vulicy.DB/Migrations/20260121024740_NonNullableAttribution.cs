using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class NonNullableAttribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feature_User_LastModifiedById",
                table: "Feature");

            migrationBuilder.AlterColumn<int>(
                name: "LastModifiedById",
                table: "FeatureHistoric",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InHistoryById",
                table: "FeatureHistoric",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LastModifiedById",
                table: "Feature",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Feature_User_LastModifiedById",
                table: "Feature",
                column: "LastModifiedById",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feature_User_LastModifiedById",
                table: "Feature");

            migrationBuilder.AlterColumn<int>(
                name: "LastModifiedById",
                table: "FeatureHistoric",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "InHistoryById",
                table: "FeatureHistoric",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "LastModifiedById",
                table: "Feature",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Feature_User_LastModifiedById",
                table: "Feature",
                column: "LastModifiedById",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
