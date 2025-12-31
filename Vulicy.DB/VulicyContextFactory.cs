using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Vulicy.DB;

public class VulicyContextFactory : IDesignTimeDbContextFactory<VulicyDbContext>
{
    public VulicyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<VulicyDbContext>();
        options.UseNpgsql(x => x.UseNetTopologySuite());
        return new VulicyDbContext(options.Options);
    }
}