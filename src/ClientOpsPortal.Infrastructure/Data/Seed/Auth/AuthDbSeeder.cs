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

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
