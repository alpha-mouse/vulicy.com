using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB.Configurations;

internal abstract class FeatureBaseConfiguration<T> : IEntityTypeConfiguration<T>
    where T: FeatureBaseEntity
{

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(x => x.NameBeTarask).HasMaxLength(128);
        builder.Property(x => x.NameBeNark).HasMaxLength(128);
        builder.Property(x => x.NameRu).HasMaxLength(128);
        builder.Property(x => x.RenamingReason).HasMaxLength(1024);
        builder.Property(x => x.HistoricNames).HasMaxLength(256);
        builder.Property(x => x.Comment).HasMaxLength(512);
        builder.Property(x => x.YearNamed).HasMaxLength(64);
        builder.Property(x => x.ForumRelativeLink).HasMaxLength(512);

        builder.Property(x => x.Geometry).HasGeometryColumnType();
    }
}

internal class FeatureConfiguration : FeatureBaseConfiguration<FeatureEntity>
{
    public const string TableName = "Feature";

    public override void Configure(EntityTypeBuilder<FeatureEntity> builder)
    {
        builder.ToTable(TableName);
        builder.UseTpcMappingStrategy();
        builder.HasKey(x => x.Id);

        base.Configure(builder);

        builder.HasTextSearchIndex(x => x.NameBeTarask);
        builder.HasTextSearchIndex(x => x.NameBeNark);
        builder.HasTextSearchIndex(x => x.NameRu);

        builder
            .HasOne(x => x.CadastreFeature)
            .WithOne(x => x.Feature)
            .HasForeignKey<CadastreFeatureEntity>(x => x.FeatureId);

        builder
            .HasMany(x => x.OsmFeatures)
            .WithOne(x => x.Feature)
            .HasForeignKey(x => x.FeatureId);

        builder.HasIndex(x => x.Geometry).HasMethod("gist");
    }
}

internal class FeatureHistoricConfiguration : FeatureBaseConfiguration<FeatureHistoricEntity>
{
    public const string TableName = "FeatureHistoric";

    public override void Configure(EntityTypeBuilder<FeatureHistoricEntity> builder)
    {
        builder.ToTable(TableName);
        builder.HasBaseType((Type?)null);
        builder.HasKey(x => new { x.Id, x.ChangeDateTime });
        base.Configure(builder);
    }
}