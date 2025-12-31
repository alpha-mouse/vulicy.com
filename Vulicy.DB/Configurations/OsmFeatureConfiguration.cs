using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB.Configurations;

internal abstract class OsmFeatureBaseConfiguration<T> : IEntityTypeConfiguration<T>
    where T : OsmFeatureBaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(x => x.Geometry).HasGeometryColumnType();
        builder.HasIndex(x => x.Geometry).HasMethod("gist");
        builder.Property(x => x.Tags).HasJsonbColumnType();
        builder.Property(x => x.IsDeleted);
    }
}

internal class OsmFeatureConfiguration : OsmFeatureBaseConfiguration<OsmFeatureEntity>
{
    public const string TableName = "OsmFeature";

    public override void Configure(EntityTypeBuilder<OsmFeatureEntity> builder)
    {
        builder.ToTable(TableName);
        builder.UseTpcMappingStrategy();
        builder.HasKey(x => new { x.Id, x.Type });
        base.Configure(builder);
    }
}

internal class OsmFeatureImportConfiguration : OsmFeatureBaseConfiguration<OsmFeatureImportEntity>
{
    public const string TableName = "OsmFeatureImport";

    public override void Configure(EntityTypeBuilder<OsmFeatureImportEntity> builder)
    {
        builder.ToTable(TableName);
        builder.HasBaseType((Type?)null);
        builder.HasKey(x => new { x.ImportId, x.Id, x.Type });

        base.Configure(builder);
    }
}

internal class OsmFeatureHistoricConfiguration : OsmFeatureBaseConfiguration<OsmFeatureHistoricEntity>
{
    public const string TableName = "OsmFeatureHistoric";

    public override void Configure(EntityTypeBuilder<OsmFeatureHistoricEntity> builder)
    {
        builder.ToTable(TableName);
        builder.HasBaseType((Type?)null);
        builder.HasKey(x => new { x.Id, x.Type, x.ChangeDateTime });
        base.Configure(builder);
    }
}