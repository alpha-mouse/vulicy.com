using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB.Configurations;

internal class FeatureConfiguration : IEntityTypeConfiguration<FeatureEntity>
{
    public const string TableName = "Feature";

    public void Configure(EntityTypeBuilder<FeatureEntity> builder)
    {
        builder.ToTable(TableName);
        builder.Property(f => f.NameBeTarask).HasMaxLength(128);
        builder.Property(f => f.NameBeNark).HasMaxLength(128);
        builder.Property(f => f.NameRu).HasMaxLength(128);
        builder.Property(f => f.RenamingReason).HasMaxLength(1024);
        builder.Property(f => f.HistoricNames).HasMaxLength(256);
        builder.Property(f => f.Comment).HasMaxLength(512);
        builder.Property(f => f.YearNamed).HasMaxLength(64);
        builder.Property(f => f.ForumRelativeLink).HasMaxLength(512);
    }
}