using ClientOpsPortal.Infrastructure.Data.Context;
using ClientOpsPortal.Infrastructure.Data.Seed.App;
using Microsoft.Extensions.Logging;

namespace ClientOpsPortal.Infrastructure.Data.Seed
{
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ClientOpsPortalDbContext _appDbContext;
        private readonly AppDbSeeder _appSeeder;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(ClientOpsPortalDbContext appDbContext, AppDbSeeder appSeeder, ILogger<DatabaseSeeder> logger)
        {
            _appDbContext = appDbContext;
            _appSeeder = appSeeder;
            _logger = logger;
        }

        public async Task SeedAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting database seeding...");
            await _appSeeder.SeedAsync(_appDbContext, cancellationToken);
            _logger.LogInformation("Database seeding completed successfully");
        }
    }
}