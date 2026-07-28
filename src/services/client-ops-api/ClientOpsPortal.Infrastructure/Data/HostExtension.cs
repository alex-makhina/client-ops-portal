using ClientOpsPortal.Infrastructure.Data.Context;
using ClientOpsPortal.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClientOpsPortal.Infrastructure.Data
{
    public static class HostExtension
    {
        public static IHost EnsureAppDatabase(this IHost host) => host.EnsureDatabase<ClientOpsPortalDbContext>();
        public static IHost EnsureAppAuthDatabase(this IHost host) => host.EnsureDatabase<AuthDbContext>();

        public static async Task<IHost> SeedDatabaseAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
            await seeder.SeedAllAsync();
            return host;
        }

        private static IHost EnsureDatabase<TDbContext>(this IHost host) where TDbContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            using var appContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("EnsureDatabase");

            var connectionString = appContext.Database.GetConnectionString();
            logger.LogInformation($"Use connectionString: '{connectionString}'");

            var created = appContext.Database.EnsureCreated();
            if (created)
                logger.LogInformation("Database created/initialized");
            else
                logger.LogInformation("Database already exists, skipping creation");

            return host;
        }
    }
}
