using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB.Configurations;

internal abstract class OsmFeatureBaseConfiguration<T> : IEntityTypeConfiguration<T>
    where T : OsmFeatureEntity
{
    protected abstract string TableName { get; }

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => new { x.OsmId, x.Type });
    }
}

internal class OsmFeatureConfiguration : OsmFeatureBaseConfiguration<OsmFeatureEntity>
{
    protected override string TableName => "OsmFeature";
}

internal class OsmFeatureImportConfiguration : OsmFeatureBaseConfiguration<OsmFeatureImportEntity>
{
    protected override string TableName => "OsmFeatureImport";
}

internal class OsmFeatureHistoricConfiguration : OsmFeatureBaseConfiguration<OsmFeatureHistoricEntity>
{
    protected override string TableName => "OsmFeatureHistoric";

    public override void Configure(EntityTypeBuilder<OsmFeatureHistoricEntity> builder)
    {
        base.Configure(builder);

        builder.HasKey(x => new { x.OsmId, x.Type, x.ChangeDateTime });
    }
}