using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB.Configurations;

internal abstract class CadastreFeatureBaseConfiguration<T> : IEntityTypeConfiguration<T>
    where T : CadastreFeatureEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(x => x.Id).HasMaxLength(128);
        builder.Property(x => x.ParentAte).HasMaxLength(256);
        builder.Property(x => x.RegionName).HasMaxLength(128);
        builder.Property(x => x.DistrictName).HasMaxLength(128);
        builder.Property(x => x.VillageCouncilName).HasMaxLength(128);
        builder.Property(x => x.AteName).HasMaxLength(128);
        builder.Property(x => x.RegionNameBel).HasMaxLength(128);
        builder.Property(x => x.DistrictNameBel).HasMaxLength(128);
        builder.Property(x => x.VillageCouncilNameBel).HasMaxLength(128);
        builder.Property(x => x.AteNameBel).HasMaxLength(128);
        builder.Property(x => x.CategoryName).HasMaxLength(128);
        builder.Property(x => x.CategoryNameShort).HasMaxLength(16);
        builder.Property(x => x.CategoryNameBel).HasMaxLength(128);
        builder.Property(x => x.CategoryNameShortBel).HasMaxLength(16);
        builder.Property(x => x.ElementTypeName).HasMaxLength(128);
        builder.Property(x => x.ElementTypeNameBel).HasMaxLength(128);
        builder.Property(x => x.ElementTypeShortName).HasMaxLength(16);
        builder.Property(x => x.ElementTypeShortNameBel).HasMaxLength(16);
        builder.Property(x => x.ElementName).HasMaxLength(512);
        builder.Property(x => x.ElementNameBel).HasMaxLength(512);
        builder.Property(x => x.ShortInfo).HasMaxLength(1024);
        builder.Property(x => x.IsDeleted);

        builder.Property(x => x.Geometry).HasGeometryColumnType();
        builder.HasIndex(x => x.Geometry).HasMethod("gist");
    }
}

internal class CadastreFeatureConfiguration : CadastreFeatureBaseConfiguration<CadastreFeatureEntity>
{
    public const string TableName = "CadastreFeature";

    public override void Configure(EntityTypeBuilder<CadastreFeatureEntity> builder)
    {
        builder.ToTable(TableName);
        builder.UseTpcMappingStrategy();
        builder.HasKey(x => x.Id);
        base.Configure(builder);
    }
}

internal class CadastreFeatureImportConfiguration : CadastreFeatureBaseConfiguration<CadastreFeatureImportEntity>
{
    public const string TableName = "CadastreFeatureImport";

    public override void Configure(EntityTypeBuilder<CadastreFeatureImportEntity> builder)
    {
        builder.ToTable(TableName);
        builder.HasBaseType((Type?)null);
        builder.HasKey(x => new { x.ImportId, x.Id });
        base.Configure(builder);
    }
}

internal class CadastreFeatureHistoricConfiguration : CadastreFeatureBaseConfiguration<CadastreFeatureHistoricEntity>
{
    public const string TableName = "CadastreFeatureHistoric";

    public override void Configure(EntityTypeBuilder<CadastreFeatureHistoricEntity> builder)
    {
        builder.ToTable(TableName);
        builder.HasBaseType((Type?)null);
        builder.HasKey(x => new { x.Id, x.ChangeDateTime });
        base.Configure(builder);
    }
}

internal class InitialCadastreFeatureImportConfiguration : IEntityTypeConfiguration<InitialCadastreFeatureImportEntity>
{
    public const string TableName = "InitialCadastreFeatureImport";

    public void Configure(EntityTypeBuilder<InitialCadastreFeatureImportEntity> builder)
    {
        builder.ToTable(TableName);

        builder.Property(x => x.Id).HasMaxLength(128);
        builder.Property(x => x.Reason).HasMaxLength(1024);
        builder.Property(x => x.HistoricName).HasMaxLength(256);
        builder.Property(x => x.Comment).HasMaxLength(512);
        builder.Property(x => x.YearNamed).HasMaxLength(64);
        builder.Property(x => x.NameCategory).HasMaxLength(256);
    }
}