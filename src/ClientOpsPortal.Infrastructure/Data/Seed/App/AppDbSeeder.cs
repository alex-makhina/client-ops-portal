using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Infrastructure.Data.Context;
using Microsoft.Extensions.Logging;

namespace ClientOpsPortal.Infrastructure.Data.Seed.App
{
    public class AppDbSeeder
    {
        private readonly ILogger<AppDbSeeder> _logger;

        public AppDbSeeder(ILogger<AppDbSeeder> logger)
        {
            _logger = logger;
        }

        public async Task SeedAsync(ClientOpsPortalDbContext context, CancellationToken cancellationToken)
        {

            if (context.Users.Any())
            {
                _logger.LogInformation("App data already exists, skipping app seed");
                return;
            }

            _logger.LogInformation("Seeding reference data...");

            var manager = new Employee
            {
                Id = Guid.Parse("a2b3c4d5-e6f7-4a8b-c9d0-e1f2a3b4c5d6"),
                FirstName = "John",
                LastName = "Smith",
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222").ToString(),
                StaffNumber = "111111",
                Post = "manager",
                CreatedAt = DateTimeOffset.UtcNow
            };
            await context.Employees.AddAsync(manager, cancellationToken);
            await context.Users.AddAsync(new User { Id = manager.UserId });

            var abonent = new Abonent
            {
                Id = Guid.Parse("a2b3c4d5-e6f7-4a8b-c9d0-e1f2a34445d6"),
                FirstName = "Иван",
                LastName = "Иванов",
                MiddleName = "Иванович",
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333").ToString(),
                IdentificationNumber = "888888888",
                AccountNumber = "1234567890",                
                CreatedAt = DateTimeOffset.UtcNow
            };
            await context.Abonents.AddAsync(abonent, cancellationToken);
            await context.Users.AddAsync(new User { Id = abonent.UserId });

            var services = new List<Service>
            {
                new() { Id = Guid.Parse("22222222-2222-2222-2222-111111111111"), Name = "Интернет", Description = "Высокоскоростной доступ в интернет", BeginDate = DateTimeOffset.UtcNow },
                new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Телевидение", Description = "150+ каналов в HD качестве", BeginDate = DateTimeOffset.UtcNow },
                new() { Id = Guid.Parse("22222222-2222-2222-2222-333333333333"), Name = "Телефония", Description = "500 минут бесплатно", BeginDate = DateTimeOffset.UtcNow }
            };
            await context.Services.AddRangeAsync(services, cancellationToken);

            var tarrifs = new List<TariffPlan>
            {
                new() { Id = Guid.Parse("22222222-2222-2222-1111-111111111111"), Name = "Премиум", Description = "100 Мбит/c", 
                    ServiceId = Guid.Parse("22222222-2222-2222-2222-111111111111"), BeginDate = DateTimeOffset.UtcNow, Price = 500 },
                new() { Id = Guid.Parse("22222222-2222-2222-1111-222222222222"), Name = "Базовый", Description = "150 каналов в HD качестве",
                    ServiceId = Guid.Parse("22222222-2222-2222-2222-222222222222"), BeginDate = DateTimeOffset.UtcNow, Price = 300 },
                new() { Id = Guid.Parse("22222222-2222-2222-1111-333333333333"), Name = "Старт", Description = "500 минут бесплатно",
                    ServiceId = Guid.Parse("22222222-2222-2222-2222-333333333333"), BeginDate = DateTimeOffset.UtcNow, Price = 200 }
            };
            await context.TariffPlans.AddRangeAsync(tarrifs, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("App data seeded");
        }
    }
}
