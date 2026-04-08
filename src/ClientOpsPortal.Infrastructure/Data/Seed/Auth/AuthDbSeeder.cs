using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClientOpsPortal.Infrastructure.Data.Seed.Auth
{
    public class AuthDbSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<AuthDbSeeder> _logger;

        public AuthDbSeeder(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<AuthDbSeeder> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync(AuthDbContext context, CancellationToken cancellationToken)
        {
            if (_userManager.Users.Any())
            {
                _logger.LogInformation("Users already exist, skipping auth seed");
                return;
            }

            _logger.LogInformation("Seeding roles and users...");

            var roles = new[] { "Admin", "Manager", "Abonent", "DataAnalyst", "ServiceManager" };
            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        Description = $"{roleName} role"
                    });
                }
            }

            var adminUser = new ApplicationUser
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
                UserName = "admin",
                Email = "admin@clientopsportal.com",
                EmailConfirmed = true
            };

            var adminResult = await _userManager.CreateAsync(adminUser, "Admin@123456");
            if (adminResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Admin user created with ID: {AdminId}", adminUser.Id);
            }

            var managerUser = new ApplicationUser
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                UserName = "manager",
                Email = "manager@clientopsportal.com",
                EmailConfirmed = true
            };

            var managerResult = await _userManager.CreateAsync(managerUser, "Manager@123456");
            if (managerResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(managerUser, "Manager");
                _logger.LogInformation("Manager user created with ID: {ManagerId}", managerUser.Id);
            }

            var abonentUser = new ApplicationUser
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                UserName = "1234567890",
                Email = "ivanov@example.com",
                EmailConfirmed = true
            };

            var userResult = await _userManager.CreateAsync(abonentUser, "User@123456");
            if (userResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(abonentUser, "Abonent");
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
