using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB.Configurations;

internal abstract class DossierRecordBaseConfiguration<T> : IEntityTypeConfiguration<T>
    where T: DossierRecordBaseEntity
{

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(x => x.NameBeTarask).HasMaxLength(128);
        builder.Property(x => x.NameBeNark).HasMaxLength(128);
        builder.Property(x => x.NameRu).HasMaxLength(128);

        builder.Property(x => x.PossibleNamesBeNark).HasJsonbColumnType();
        builder.Property(x => x.PossibleNamesRu).HasJsonbColumnType();
        builder.Property(x => x.AlternativeDescriptionsBe).HasJsonbColumnType();
        builder.Property(x => x.AlternativeDescriptionsRu).HasJsonbColumnType();
    }
}

internal class DossierRecordConfiguration : DossierRecordBaseConfiguration<DossierRecordEntity>
{
    public const string TableName = "DossierRecord";

    public override void Configure(EntityTypeBuilder<DossierRecordEntity> builder)
    {
        builder.ToTable(TableName);
        builder.UseTpcMappingStrategy();
        builder.HasKey(x => x.Id);
        base.Configure(builder);

        builder.HasTextSearchIndex(x => x.NameBeTarask);
        builder.HasTextSearchIndex(x => x.NameBeNark);
        builder.HasTextSearchIndex(x => x.NameRu);
    }
}

internal class DossierRecordHistoricConfiguration : DossierRecordBaseConfiguration<DossierRecordHistoricEntity>
{
    public const string TableName = "DossierRecordHistoric";

    public override void Configure(EntityTypeBuilder<DossierRecordHistoricEntity> builder)
    {
        builder.ToTable(TableName);
        builder.HasBaseType((Type?)null);
        builder.HasKey(x => new { x.Id, x.ChangeDateTime });
        base.Configure(builder);
    }
}