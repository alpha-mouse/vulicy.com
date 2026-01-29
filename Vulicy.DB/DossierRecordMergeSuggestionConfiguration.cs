using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vulicy.Domain;

namespace Vulicy.DB;

public class DossierRecordMergeSuggestionConfiguration : IEntityTypeConfiguration<DossierRecordMergeSuggestionEntity>
{
    public const string TableName = "DossierRecordMergeSuggestion";

    public void Configure(EntityTypeBuilder<DossierRecordMergeSuggestionEntity> builder)
    {
        builder.ToTable(TableName);

        builder
            .HasOne(x => x.LeftRecord)
            .WithMany()
            .HasForeignKey(x => x.LeftRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.RightRecord)
            .WithMany()
            .HasForeignKey(x => x.RightRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}