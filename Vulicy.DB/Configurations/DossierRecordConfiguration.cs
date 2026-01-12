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
        builder.Property(x => x.NameBeTarask).HasMaxLength(128);
        builder.Property(x => x.NameBeNark).HasMaxLength(128);
        builder.Property(x => x.NameRu).HasMaxLength(128);

        builder.Property(x => x.PossibleNamesBeNark).HasJsonColumnType();
        builder.Property(x => x.PossibleNamesRu).HasJsonColumnType();
    }
}