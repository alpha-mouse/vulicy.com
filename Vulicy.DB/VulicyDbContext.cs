using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Vulicy.Domain;

namespace Vulicy.DB;

public class VulicyDbContext : DbContext
{
    public VulicyDbContext(DbContextOptions<VulicyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override int SaveChanges()
    {
        OnBeforeSaving();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries();
        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.Entity is IEntity entity)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entity.ModifiedDateTime = utcNow;
                        break;

                    case EntityState.Added:
                        entity.CreatedDateTime = utcNow;
                        entity.ModifiedDateTime = utcNow;
                        break;
                }
            }
        }
    }
}