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

            var serviceManagerUser = new ApplicationUser
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                UserName = "smanager",
                Email = "smanager@clientopsportal.com",
                EmailConfirmed = true
            };

            var serviceManagerResult = await _userManager.CreateAsync(serviceManagerUser, "Smanager@123456");
            if (serviceManagerResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(serviceManagerUser, "ServiceManager");
                _logger.LogInformation("Service Manager user created with ID: {ServiceManagerUser}", serviceManagerUser.Id);
            }

            var managerUser = new ApplicationUser
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111113"),
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

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
