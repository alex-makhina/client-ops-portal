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

            var adminUserRecord = new User
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ExternalId = "11111111-1111-1111-1111-111111111111",
                IdentityProvider = "Identity",
                Email = "admin@clientopsportal.com"
            };
            await context.Users.AddAsync(adminUserRecord, cancellationToken);

            var managerUserRecord = new User
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                ExternalId = "22222222-2222-2222-2222-222222222222",
                IdentityProvider = "Identity",
                Email = "manager@clientopsportal.com"
            };
            await context.Users.AddAsync(managerUserRecord, cancellationToken);

            var abonentUserRecord = new User
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                ExternalId = "33333333-3333-3333-3333-333333333333",
                IdentityProvider = "Identity",
                Email = "ivanov@example.com"
            };
            await context.Users.AddAsync(abonentUserRecord, cancellationToken);

            var adminEmployee = new Employee
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
                FirstName = "Admin",
                LastName = "Super",
                MiddleName = "System",
                UserId = adminUserRecord.Id,
                StaffNumber = "000001",
                Post = "Администратор",
                Department = "IT",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await context.Employees.AddAsync(adminEmployee, cancellationToken);

            var manager = new Employee
            {
                Id = Guid.Parse("a2b3c4d5-e6f7-4a8b-c9d0-e1f2a3b4c5d6"),
                FirstName = "John",
                LastName = "Smith",
                UserId = managerUserRecord.Id,
                StaffNumber = "111111",
                Post = "manager",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await context.Employees.AddAsync(manager, cancellationToken);

            var abonent = new Abonent
            {
                Id = Guid.Parse("a2b3c4d5-e6f7-4a8b-c9d0-e1f2a34445d6"),
                FirstName = "Иван",
                LastName = "Иванов",
                MiddleName = "Иванович",
                UserId = abonentUserRecord.Id,
                IdentificationNumber = "888888888",
                AccountNumber = "1234567890",
                CreatedAt = DateTimeOffset.UtcNow
            };
            await context.Abonents.AddAsync(abonent, cancellationToken);

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
