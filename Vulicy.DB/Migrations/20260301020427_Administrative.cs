using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class Administrative : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Administrative",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NameBeTarask = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameBeNark = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameRu = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    ParentRegionId = table.Column<int>(type: "integer", nullable: true),
                    ParentDistrictId = table.Column<int>(type: "integer", nullable: true),
                    ParentVillageCouncilId = table.Column<int>(type: "integer", nullable: true),
                    CadastreAte = table.Column<int>(type: "integer", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administrative", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Administrative_Administrative_ParentDistrictId",
                        column: x => x.ParentDistrictId,
                        principalTable: "Administrative",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Administrative_Administrative_ParentRegionId",
                        column: x => x.ParentRegionId,
                        principalTable: "Administrative",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Administrative_Administrative_ParentVillageCouncilId",
                        column: x => x.ParentVillageCouncilId,
                        principalTable: "Administrative",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Administrative_CadastreAte",
                table: "Administrative",
                column: "CadastreAte",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Administrative_ParentDistrictId",
                table: "Administrative",
                column: "ParentDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Administrative_ParentRegionId",
                table: "Administrative",
                column: "ParentRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Administrative_ParentVillageCouncilId",
                table: "Administrative",
                column: "ParentVillageCouncilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Administrative");
        }
    }
}
