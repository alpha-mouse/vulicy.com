using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class DossierRecordsIndexingAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "DossierRecordEntitySequence");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DossierRecord",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('\"DossierRecordEntitySequence\"')",
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedById",
                table: "DossierRecord",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DossierRecordHistoric",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ChangeDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InHistoryById = table.Column<int>(type: "integer", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NameBeTarask = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    NameBeNark = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    NameRu = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DescriptionBe = table.Column<string>(type: "text", nullable: true),
                    DescriptionRu = table.Column<string>(type: "text", nullable: true),
                    PossibleNamesBeNark = table.Column<string>(type: "json", nullable: true),
                    PossibleNamesRu = table.Column<string>(type: "json", nullable: true),
                    Classification = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedById = table.Column<int>(type: "integer", nullable: false),
                    NamingCategoryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DossierRecordHistoric", x => new { x.Id, x.ChangeDateTime });
                });

            migrationBuilder.CreateIndex(
                name: "IX_DossierRecord_LastModifiedById",
                table: "DossierRecord",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_DossierRecord_NameBeNark",
                table: "DossierRecord",
                column: "NameBeNark")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_DossierRecord_NameBeTarask",
                table: "DossierRecord",
                column: "NameBeTarask")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_DossierRecord_NameRu",
                table: "DossierRecord",
                column: "NameRu")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.AddForeignKey(
                name: "FK_DossierRecord_User_LastModifiedById",
                table: "DossierRecord",
                column: "LastModifiedById",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DossierRecord_User_LastModifiedById",
                table: "DossierRecord");

            migrationBuilder.DropTable(
                name: "DossierRecordHistoric");

            migrationBuilder.DropIndex(
                name: "IX_DossierRecord_LastModifiedById",
                table: "DossierRecord");

            migrationBuilder.DropIndex(
                name: "IX_DossierRecord_NameBeNark",
                table: "DossierRecord");

            migrationBuilder.DropIndex(
                name: "IX_DossierRecord_NameBeTarask",
                table: "DossierRecord");

            migrationBuilder.DropIndex(
                name: "IX_DossierRecord_NameRu",
                table: "DossierRecord");

            migrationBuilder.DropColumn(
                name: "LastModifiedById",
                table: "DossierRecord");

            migrationBuilder.DropSequence(
                name: "DossierRecordEntitySequence");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DossierRecord",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "nextval('\"DossierRecordEntitySequence\"')")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
