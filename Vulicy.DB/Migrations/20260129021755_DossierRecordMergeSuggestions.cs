using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class DossierRecordMergeSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DossierRecordMergeSuggestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeftRecordId = table.Column<int>(type: "integer", nullable: false),
                    RightRecordId = table.Column<int>(type: "integer", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DossierRecordMergeSuggestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DossierRecordMergeSuggestion_DossierRecord_LeftRecordId",
                        column: x => x.LeftRecordId,
                        principalTable: "DossierRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DossierRecordMergeSuggestion_DossierRecord_RightRecordId",
                        column: x => x.RightRecordId,
                        principalTable: "DossierRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DossierRecordMergeSuggestion_LeftRecordId",
                table: "DossierRecordMergeSuggestion",
                column: "LeftRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DossierRecordMergeSuggestion_RightRecordId",
                table: "DossierRecordMergeSuggestion",
                column: "RightRecordId");

            migrationBuilder.Sql(
                """
                with "FeatureCounts" as (
                    select f."DossierRecordId" as "Id", count(*) as "FeaturesCount"
                    from "Feature" f 
                    group by f."DossierRecordId" 
                )
                insert into "DossierRecordMergeSuggestion" ("LeftRecordId", "RightRecordId", "CreatedDateTime", "ModifiedDateTime")
                select dr1."Id", dr2."Id", now(), now()
                from "DossierRecord" dr1
                cross join "DossierRecord" dr2
                left join "FeatureCounts" fc1 on dr1."Id" = fc1."Id" 
                left join "FeatureCounts" fc2 on dr2."Id" = fc2."Id" 
                where dr1."Id" < dr2."Id"
                    and ( dr1."NameBeTarask" = dr2."NameBeTarask"
                       or dr1."NameBeNark" = dr2."NameBeNark"
                       or dr1."NameRu" = dr2."NameRu")
                order by coalesce(fc1."FeaturesCount" , 0) + coalesce(fc2."FeaturesCount" , 0) desc
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DossierRecordMergeSuggestion");
        }
    }
}
