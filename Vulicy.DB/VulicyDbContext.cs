using Microsoft.EntityFrameworkCore;
using System.Reflection;

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

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}