using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB.Configurations;

internal abstract class CadastreFeatureBaseConfiguration<T> : IEntityTypeConfiguration<T>
    where T : CadastreFeatureEntity
{
    protected abstract string TableName { get; }

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.CadastreId);
        builder.Property(f => f.CadastreId).HasMaxLength(128);
        builder.Property(f => f.ParentAte).HasMaxLength(256);
        builder.Property(f => f.RegionName).HasMaxLength(128);
        builder.Property(f => f.DistrictName).HasMaxLength(128);
        builder.Property(f => f.VillageCouncilName).HasMaxLength(128);
        builder.Property(f => f.AteName).HasMaxLength(128);
        builder.Property(f => f.RegionNameBel).HasMaxLength(128);
        builder.Property(f => f.DistrictNameBel).HasMaxLength(128);
        builder.Property(f => f.VillageCouncilNameBel).HasMaxLength(128);
        builder.Property(f => f.AteNameBel).HasMaxLength(128);
        builder.Property(f => f.CategoryName).HasMaxLength(128);
        builder.Property(f => f.CategoryNameShort).HasMaxLength(16);
        builder.Property(f => f.CategoryNameBel).HasMaxLength(128);
        builder.Property(f => f.CategoryNameShortBel).HasMaxLength(16);
        builder.Property(f => f.ElementTypeName).HasMaxLength(128);
        builder.Property(f => f.ElementTypeNameBel).HasMaxLength(128);
        builder.Property(f => f.ElementTypeShortName).HasMaxLength(16);
        builder.Property(f => f.ElementTypeShortNameBel).HasMaxLength(16);
        builder.Property(f => f.ElementName).HasMaxLength(512);
        builder.Property(f => f.ElementNameBel).HasMaxLength(512);
        builder.Property(f => f.ShortInfo).HasMaxLength(1024);
    }
}

internal class CadastreFeatureConfiguration : CadastreFeatureBaseConfiguration<CadastreFeatureEntity>
{
    protected override string TableName => "CadastreFeature";
}

internal class CadastreFeatureImportConfiguration : CadastreFeatureBaseConfiguration<CadastreFeatureImportEntity>
{
    protected override string TableName => "CadastreFeatureImport";
}

internal class CadastreFeatureHistoricConfiguration : CadastreFeatureBaseConfiguration<CadastreFeatureHistoricEntity>
{
    protected override string TableName => "CadastreFeatureHistoric";

    public override void Configure(EntityTypeBuilder<CadastreFeatureHistoricEntity> builder)
    {
        base.Configure(builder);

        builder.HasKey(x => new { x.CadastreId, x.ChangeDateTime });
    }
}

internal class InitialCadastreFeatureConfiguration : CadastreFeatureBaseConfiguration<InitialCadastreFeatureImportEntity>
{
    protected override string TableName => "InitialCadastreFeatureImport";

    public override void Configure(EntityTypeBuilder<InitialCadastreFeatureImportEntity> builder)
    {
        base.Configure(builder);

        builder.Property(f => f.Reason).HasMaxLength(1024);
        builder.Property(f => f.HistoricName).HasMaxLength(256);
        builder.Property(f => f.Comment).HasMaxLength(512);
        builder.Property(f => f.YearNamed).HasMaxLength(64);
        builder.Property(f => f.NameCategory).HasMaxLength(256);
    }
}