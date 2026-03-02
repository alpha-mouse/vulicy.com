using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB.Configurations;

public class AdministrativeConfiguration : IEntityTypeConfiguration<AdministrativeEntity>
{
    public const string TableName = "Administrative";

    public void Configure(EntityTypeBuilder<AdministrativeEntity> builder)
    {
        builder.ToTable(TableName);

        builder.Property(x => x.NameBeTarask).HasMaxLength(128);
        builder.Property(x => x.NameBeNark).HasMaxLength(128);
        builder.Property(x => x.NameRu).HasMaxLength(128);

        builder
            .HasOne<AdministrativeEntity>()
            .WithMany()
            .HasForeignKey(x => x.ParentRegionId);

        builder
            .HasOne<AdministrativeEntity>()
            .WithMany()
            .HasForeignKey(x => x.ParentDistrictId);

        builder
            .HasOne<AdministrativeEntity>()
            .WithMany()
            .HasForeignKey(x => x.ParentVillageCouncilId);

        builder.HasIndex(x => x.CadastreAte).IsUnique();
    }
}