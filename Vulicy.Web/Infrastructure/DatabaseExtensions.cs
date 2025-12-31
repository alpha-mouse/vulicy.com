using Microsoft.EntityFrameworkCore;
using Npgsql;
using Vulicy.DB;

namespace Vulicy.Web.Infrastructure;

public static class DatabaseExtensions
{
    public static void AddDatabase(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddConventionalServices(typeof(RepositoryBase<>).Assembly);
        serviceCollection.AddDbContext(configuration);
    }

    public static async Task InitializeDatabases(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<VulicyDbContext>();
        await context.Database.MigrateAsync();
    }

    public static void AddDbContext(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var npgsqlDataSource = new NpgsqlDataSourceBuilder(connectionString).Build();

        serviceCollection.AddDbContext<VulicyDbContext>(options =>
        {
            options.UseNpgsql(npgsqlDataSource, action =>
            {
                action.CommandTimeout(60);
                action.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
    }
}