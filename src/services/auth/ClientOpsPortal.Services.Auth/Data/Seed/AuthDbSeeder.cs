using ClientOpsPortal.Services.Auth.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
namespace ClientOpsPortal.Services.Auth.Data.Seed;
public class AuthDbSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<AuthDbSeeder> _logger;
    public AuthDbSeeder(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ILogger<AuthDbSeeder> logger)
    { _userManager = userManager; _roleManager = roleManager; _logger = logger; }
    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested) return;
        var roles = new[] { "Admin", "Manager", "Abonent", "DataAnalyst", "ServiceManager" };
        foreach (var roleName in roles)
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName, Description = roleName });
                _logger.LogInformation("Created role {RoleName}", roleName);
            }
        if (await _userManager.FindByNameAsync("admin") == null)
        {
            var adminUser = new ApplicationUser { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), UserName = "admin", Email = "admin@clientopsportal.com", EmailConfirmed = true };
            if ((await _userManager.CreateAsync(adminUser, "Admin@123456")).Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Admin user created {UserId} {UserName}", adminUser.Id, adminUser.UserName);
            }
            var sm = new ApplicationUser { UserName = "smanager", Email = "smanager@clientopsportal.com", EmailConfirmed = true };
            if ((await _userManager.CreateAsync(sm, "Smanager@123456")).Succeeded)
            {
                await _userManager.AddToRoleAsync(sm, "ServiceManager");
                _logger.LogInformation("Service manager user created {UserId} {UserName}", sm.Id, sm.UserName);
            }
            var mgr = new ApplicationUser { UserName = "manager", Email = "manager@clientopsportal.com", EmailConfirmed = true };
            if ((await _userManager.CreateAsync(mgr, "Manager@123456")).Succeeded)
            {
                await _userManager.AddToRoleAsync(mgr, "Manager");
                _logger.LogInformation("Manager user created {UserId} {UserName}", mgr.Id, mgr.UserName);
            }
        }
    }
}