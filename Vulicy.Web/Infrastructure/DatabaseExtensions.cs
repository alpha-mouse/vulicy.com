using Microsoft.EntityFrameworkCore;
using Npgsql;
using Vulicy.DB;

namespace Vulicy.Web.Infrastructure;

public static class DatabaseExtensions
{
    public static void AddDatabase(this IServiceCollection serviceCollection, IConfiguration configuration, IWebHostEnvironment environment)
    {
        serviceCollection.AddConventionalServices(typeof(VulicyDbContext).Assembly);
        serviceCollection.AddDbContext(configuration, environment);
    }

    public static async Task InitializeDatabases(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<VulicyDbContext>();
        await context.Database.MigrateAsync();

        // Warm up connection pool to avoid cold start delays on first requests
        await context.Database.ExecuteSqlRawAsync("SELECT 1");
    }

    public static void AddDbContext(this IServiceCollection serviceCollection, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var npgsqlDataSource = new NpgsqlDataSourceBuilder(connectionString)
            .UseNetTopologySuite()
            .EnableDynamicJson()
            .Build();

        serviceCollection.AddDbContext<VulicyDbContext>(options =>
        {
            options.UseNpgsql(npgsqlDataSource, action =>
            {
                // Default ceiling for interactive queries; import work opts out via DbCommandTimeout.Unlimited().
                action.CommandTimeout(30);
                action.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                action.UseNetTopologySuite();
            });

            options.AddInterceptors(new CommandTimeoutInterceptor());

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });
    }
}