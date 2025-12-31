using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB.Configurations;

public class ImportConfiguration : IEntityTypeConfiguration<ImportEntity>
{
    public const string TableName = "Import";

    public void Configure(EntityTypeBuilder<ImportEntity> builder)
    {
        builder.ToTable(TableName);
    }
}