using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB.Configurations;

internal class NamingCategoryConfiguration : IEntityTypeConfiguration<NamingCategoryEntity>
{
    public const string TableName = "NamingCategory";

    public void Configure(EntityTypeBuilder<NamingCategoryEntity> builder)
    {
        builder.ToTable(TableName);
        builder.Property(f => f.Name).HasMaxLength(256);
    }
}