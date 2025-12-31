using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB.Configurations;

internal class DossierRecordConfiguration : IEntityTypeConfiguration<DossierRecordEntity>
{
    public const string TableName = "DossierRecord";

    public void Configure(EntityTypeBuilder<DossierRecordEntity> builder)
    {
        builder.ToTable(TableName);
        builder.Property(f => f.NameBeTarask).HasMaxLength(128);
        builder.Property(f => f.NameBeNark).HasMaxLength(128);
        builder.Property(f => f.NameRu).HasMaxLength(128);
    }
}