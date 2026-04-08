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
        public static IHost MigrateAppDatabase(this IHost host) => host.MigrateDatabase<ClientOpsPortalDbContext>();
        public static IHost MigrateAppAuthDatabase(this IHost host) => host.MigrateDatabase<AuthDbContext>();

        public static async Task<IHost> SeedDatabaseAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
            await seeder.SeedAllAsync();
            return host;
        }

        private static IHost MigrateDatabase<TDbContext>(this IHost host) where TDbContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            using var appContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("MigrateDatabase");

            var connectionString = appContext.Database.GetConnectionString();
            logger.LogInformation($"Use connectionString: '{connectionString}'");

            var pendingMigrations = appContext.Database.GetPendingMigrations().ToArray();
            var message = pendingMigrations.Length > 0
                ? $"There are pending migrations '{string.Join(", ", pendingMigrations)}'"
                : "No pending migrations";

            logger.LogInformation(message);

            appContext.Database.Migrate();
            return host;
        }
    }
}
