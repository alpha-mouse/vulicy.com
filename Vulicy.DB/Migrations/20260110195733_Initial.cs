using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Vulicy.DB.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateSequence(
                name: "FeatureEntitySequence");

            migrationBuilder.CreateTable(
                name: "Import",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DownloadUrl = table.Column<string>(type: "text", nullable: false),
                    LocalPath = table.Column<string>(type: "text", nullable: false),
                    Cleared = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Import", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InitialCadastreFeatureImport",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Classification = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    HistoricName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NameCategory = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Comment = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    HistoricPossible = table.Column<bool>(type: "boolean", nullable: false),
                    YearNamed = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitialCadastreFeatureImport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NamingCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NamingCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DossierRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NameBeTarask = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    NameBeNark = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    NameRu = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DescriptionBe = table.Column<string>(type: "text", nullable: true),
                    DescriptionRu = table.Column<string>(type: "text", nullable: true),
                    Classification = table.Column<int>(type: "integer", nullable: false),
                    NamingCategoryId = table.Column<int>(type: "integer", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DossierRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DossierRecord_NamingCategory_NamingCategoryId",
                        column: x => x.NamingCategoryId,
                        principalTable: "NamingCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Feature",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"FeatureEntitySequence\"')"),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NameBeTarask = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameBeNark = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameRu = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Classification = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    RenamingReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    HistoricNames = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Comment = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    HistoricPossible = table.Column<bool>(type: "boolean", nullable: false),
                    YearNamed = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ForumRelativeLink = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Geometry = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: false),
                    NamingCategoryId = table.Column<int>(type: "integer", nullable: true),
                    DossierRecordId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feature_DossierRecord_DossierRecordId",
                        column: x => x.DossierRecordId,
                        principalTable: "DossierRecord",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Feature_NamingCategory_NamingCategoryId",
                        column: x => x.NamingCategoryId,
                        principalTable: "NamingCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeatureHistoric",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ChangeDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NameBeTarask = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameBeNark = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameRu = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Classification = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    RenamingReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    HistoricNames = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Comment = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    HistoricPossible = table.Column<bool>(type: "boolean", nullable: false),
                    YearNamed = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ForumRelativeLink = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Geometry = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: false),
                    NamingCategoryId = table.Column<int>(type: "integer", nullable: true),
                    DossierRecordId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureHistoric", x => new { x.Id, x.ChangeDateTime });
                    table.ForeignKey(
                        name: "FK_FeatureHistoric_DossierRecord_DossierRecordId",
                        column: x => x.DossierRecordId,
                        principalTable: "DossierRecord",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeatureHistoric_NamingCategory_NamingCategoryId",
                        column: x => x.NamingCategoryId,
                        principalTable: "NamingCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CadastreFeature",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FeatureId = table.Column<int>(type: "integer", nullable: true),
                    Geometry = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: true),
                    IdIae = table.Column<int>(type: "integer", nullable: false),
                    ParentAte = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Region = table.Column<int>(type: "integer", nullable: true),
                    District = table.Column<int>(type: "integer", nullable: true),
                    VillageCouncil = table.Column<int>(type: "integer", nullable: true),
                    Ate = table.Column<int>(type: "integer", nullable: false),
                    RegionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DistrictName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VillageCouncilName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AteName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RegionNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DistrictNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VillageCouncilNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AteNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CategoryName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CategoryNameShort = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    CategoryNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CategoryNameShortBel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ElementType = table.Column<int>(type: "integer", nullable: false),
                    ElementTypeName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ElementTypeNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ElementTypeShortName = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ElementTypeShortNameBel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ElementName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ElementNameBel = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ShortInfo = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ObjectNumber = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CadastreFeature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CadastreFeature_Feature_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Feature",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CadastreFeatureHistoric",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ChangeDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FeatureId = table.Column<int>(type: "integer", nullable: true),
                    Geometry = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: true),
                    IdIae = table.Column<int>(type: "integer", nullable: false),
                    ParentAte = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Region = table.Column<int>(type: "integer", nullable: true),
                    District = table.Column<int>(type: "integer", nullable: true),
                    VillageCouncil = table.Column<int>(type: "integer", nullable: true),
                    Ate = table.Column<int>(type: "integer", nullable: false),
                    RegionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DistrictName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VillageCouncilName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AteName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RegionNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DistrictNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VillageCouncilNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AteNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CategoryName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CategoryNameShort = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    CategoryNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CategoryNameShortBel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ElementType = table.Column<int>(type: "integer", nullable: false),
                    ElementTypeName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ElementTypeNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ElementTypeShortName = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ElementTypeShortNameBel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ElementName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ElementNameBel = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ShortInfo = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ObjectNumber = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CadastreFeatureHistoric", x => new { x.Id, x.ChangeDateTime });
                    table.ForeignKey(
                        name: "FK_CadastreFeatureHistoric_Feature_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Feature",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CadastreFeatureImport",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ImportId = table.Column<int>(type: "integer", nullable: false),
                    DoUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FeatureId = table.Column<int>(type: "integer", nullable: true),
                    Geometry = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: true),
                    IdIae = table.Column<int>(type: "integer", nullable: false),
                    ParentAte = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Region = table.Column<int>(type: "integer", nullable: true),
                    District = table.Column<int>(type: "integer", nullable: true),
                    VillageCouncil = table.Column<int>(type: "integer", nullable: true),
                    Ate = table.Column<int>(type: "integer", nullable: false),
                    RegionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DistrictName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VillageCouncilName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AteName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RegionNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DistrictNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VillageCouncilNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AteNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CategoryName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CategoryNameShort = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    CategoryNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CategoryNameShortBel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ElementType = table.Column<int>(type: "integer", nullable: false),
                    ElementTypeName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ElementTypeNameBel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ElementTypeShortName = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ElementTypeShortNameBel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ElementName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ElementNameBel = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ShortInfo = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ObjectNumber = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CadastreFeatureImport", x => new { x.ImportId, x.Id });
                    table.ForeignKey(
                        name: "FK_CadastreFeatureImport_Feature_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Feature",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OsmFeature",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    GeometryUpdatePending = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FeatureId = table.Column<int>(type: "integer", nullable: true),
                    Geometry = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: false),
                    Tags = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OsmFeature", x => new { x.Id, x.Type });
                    table.ForeignKey(
                        name: "FK_OsmFeature_Feature_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Feature",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OsmFeatureHistoric",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ChangeDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FeatureId = table.Column<int>(type: "integer", nullable: true),
                    Geometry = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: false),
                    Tags = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OsmFeatureHistoric", x => new { x.Id, x.Type, x.ChangeDateTime });
                    table.ForeignKey(
                        name: "FK_OsmFeatureHistoric_Feature_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Feature",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OsmFeatureImport",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ImportId = table.Column<int>(type: "integer", nullable: false),
                    DoUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FeatureId = table.Column<int>(type: "integer", nullable: true),
                    Geometry = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: false),
                    Tags = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OsmFeatureImport", x => new { x.ImportId, x.Id, x.Type });
                    table.ForeignKey(
                        name: "FK_OsmFeatureImport_Feature_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Feature",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CadastreFeature_FeatureId",
                table: "CadastreFeature",
                column: "FeatureId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CadastreFeature_Geometry",
                table: "CadastreFeature",
                column: "Geometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_CadastreFeatureHistoric_FeatureId",
                table: "CadastreFeatureHistoric",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_CadastreFeatureHistoric_Geometry",
                table: "CadastreFeatureHistoric",
                column: "Geometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_CadastreFeatureImport_FeatureId",
                table: "CadastreFeatureImport",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_CadastreFeatureImport_Geometry",
                table: "CadastreFeatureImport",
                column: "Geometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_DossierRecord_NamingCategoryId",
                table: "DossierRecord",
                column: "NamingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Feature_DossierRecordId",
                table: "Feature",
                column: "DossierRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Feature_Geometry",
                table: "Feature",
                column: "Geometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_Feature_NameBeNark",
                table: "Feature",
                column: "NameBeNark")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Feature_NameBeTarask",
                table: "Feature",
                column: "NameBeTarask")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Feature_NameRu",
                table: "Feature",
                column: "NameRu")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Feature_NamingCategoryId",
                table: "Feature",
                column: "NamingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureHistoric_DossierRecordId",
                table: "FeatureHistoric",
                column: "DossierRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureHistoric_NamingCategoryId",
                table: "FeatureHistoric",
                column: "NamingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OsmFeature_FeatureId",
                table: "OsmFeature",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_OsmFeature_Geometry",
                table: "OsmFeature",
                column: "Geometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_OsmFeatureHistoric_FeatureId",
                table: "OsmFeatureHistoric",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_OsmFeatureHistoric_Geometry",
                table: "OsmFeatureHistoric",
                column: "Geometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_OsmFeatureImport_FeatureId",
                table: "OsmFeatureImport",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_OsmFeatureImport_Geometry",
                table: "OsmFeatureImport",
                column: "Geometry")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CadastreFeature");

            migrationBuilder.DropTable(
                name: "CadastreFeatureHistoric");

            migrationBuilder.DropTable(
                name: "CadastreFeatureImport");

            migrationBuilder.DropTable(
                name: "FeatureHistoric");

            migrationBuilder.DropTable(
                name: "Import");

            migrationBuilder.DropTable(
                name: "InitialCadastreFeatureImport");

            migrationBuilder.DropTable(
                name: "OsmFeature");

            migrationBuilder.DropTable(
                name: "OsmFeatureHistoric");

            migrationBuilder.DropTable(
                name: "OsmFeatureImport");

            migrationBuilder.DropTable(
                name: "Feature");

            migrationBuilder.DropTable(
                name: "DossierRecord");

            migrationBuilder.DropTable(
                name: "NamingCategory");

            migrationBuilder.DropSequence(
                name: "FeatureEntitySequence");
        }
    }
}
