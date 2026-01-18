using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public const string TableName = "User";

    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable(TableName);
        builder.HasIndex(x => x.ExternalId).IsUnique();
        builder.Property(x => x.Username).HasMaxLength(256);
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.Name).HasMaxLength(512);
        builder.Property(x => x.AvatarUrl).HasMaxLength(1024);
    }
}
