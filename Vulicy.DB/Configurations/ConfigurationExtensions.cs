using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vulicy.DB.Configurations;

public static class ConfigurationExtensions
{
    extension<T>(PropertyBuilder<T> propertyBuilder)
    {
        public PropertyBuilder<T> HasGeometryColumnType()
            => propertyBuilder.HasColumnType("geometry(Geometry, 4326)");

        public PropertyBuilder<T> HasJsonbColumnType()
            => propertyBuilder.HasColumnType("jsonb");
    }

    extension<T>(EntityTypeBuilder<T> entityBuilder) where T : class
    {
        public IndexBuilder<T> HasTextSearchIndex(Expression<Func<T, object?>> indexExpression)
            => entityBuilder
                .HasIndex(indexExpression)
                .HasMethod("GIN")
                .HasOperators("gin_trgm_ops");
    }
}