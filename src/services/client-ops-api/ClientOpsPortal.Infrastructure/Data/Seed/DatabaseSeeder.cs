using ClientOpsPortal.Infrastructure.Data.Context;
using ClientOpsPortal.Infrastructure.Data.Seed.App;
using ClientOpsPortal.Infrastructure.Data.Seed.Auth;
using Microsoft.Extensions.Logging;

namespace ClientOpsPortal.Infrastructure.Data.Seed
{
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly AuthDbContext _authDbContext;
        private readonly ClientOpsPortalDbContext _appDbContext;
        private readonly AuthDbSeeder _authSeeder;
        private readonly AppDbSeeder _appSeeder;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            AuthDbContext authDbContext,
            ClientOpsPortalDbContext appDbContext,
            AuthDbSeeder authSeeder,
            AppDbSeeder appSeeder,
            ILogger<DatabaseSeeder> logger)
        {
            _authDbContext = authDbContext;
            _appDbContext = appDbContext;
            _authSeeder = authSeeder;
            _appSeeder = appSeeder;
            _logger = logger;
        }

        public async Task SeedAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting database seeding...");

            await _authSeeder.SeedAsync(_authDbContext, cancellationToken);

            await _appSeeder.SeedAsync(_appDbContext, cancellationToken);

            _logger.LogInformation("Database seeding completed successfully");
        }
    }
}
