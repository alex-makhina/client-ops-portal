using ClientOpsPortal.Services.Auth.Contracts;
using ClientOpsPortal.Services.Auth.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenIddict.Validation.AspNetCore;

namespace ClientOpsPortal.Services.Auth.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = _userManager.Users.ToList();
        var result = new List<UserResponse>();
        foreach (var u in users)
        {
            var roles = (await _userManager.GetRolesAsync(u)).ToList();
            result.Add(new UserResponse
            {
                Id = u.Id.ToString(), UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty, IsBlocked = await _userManager.IsLockedOutAsync(u), Roles = roles
            });
        }

        _logger.LogInformation("User list retrieved: {UserCount} users", result.Count);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", id);
            return NotFound();
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        _logger.LogDebug("User {UserId} retrieved", user.Id);
        return Ok(new UserResponse
        {
            Id = user.Id.ToString(), UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty, IsBlocked = await _userManager.IsLockedOutAsync(user), Roles = roles
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var isAdmin = User.IsInRole("Admin");
        var requestedRoles = request.Roles ?? [];

        if (!isAdmin && !requestedRoles.All(r => r == "Abonent"))
        {
            _logger.LogWarning("User creation denied for non-admin user {UserName} requesting roles {Roles}", User?.Identity?.Name, requestedRoles);
            return Forbid();
        }

        var user = new ApplicationUser { UserName = request.UserName, Email = request.Email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("User creation failed for {UserName}: {Errors}", request.UserName, string.Join("; ", result.Errors.Select(e => e.Description)));
            return BadRequest(result.Errors);
        }

        foreach (var role in requestedRoles)
            if (await _roleManager.RoleExistsAsync(role))
                await _userManager.AddToRoleAsync(user, role);

        _logger.LogInformation("User {UserName} created with id {UserId} and roles {Roles} by {CreatedBy}",
            request.UserName, user.Id, requestedRoles, User?.Identity?.Name);
        return Ok(new CreateUserResponse { UserId = user.Id.ToString() });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/role")]
    public async Task<IActionResult> SetRole(Guid id, [FromBody] SetUserRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            _logger.LogWarning("Role change failed: user {UserId} not found", id);
            return NotFound();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (await _roleManager.RoleExistsAsync(request.Role))
            await _userManager.AddToRoleAsync(user, request.Role);

        _logger.LogInformation("User {UserId} role changed to {Role} by {ChangedBy}", user.Id, request.Role, User?.Identity?.Name);
        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/block")]
    public async Task<IActionResult> Block(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            _logger.LogWarning("Block failed: user {UserId} not found", id);
            return NotFound();
        }

        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        _logger.LogInformation("User {UserId} blocked by {BlockedBy}", user.Id, User?.Identity?.Name);
        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/unblock")]
    public async Task<IActionResult> Unblock(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            _logger.LogWarning("Unblock failed: user {UserId} not found", id);
            return NotFound();
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        _logger.LogInformation("User {UserId} unblocked by {UnblockedBy}", user.Id, User?.Identity?.Name);
        return Ok();
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("random-password")]
    public IActionResult GenerateRandomPassword()
    {
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "!@#$%^&*()-_=+";
        const string all = lower + upper + digits + special;

        var random = new Random();
        var password = new char[12];

        password[0] = lower[random.Next(lower.Length)];
        password[1] = upper[random.Next(upper.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        for (int i = 4; i < password.Length; i++)
        {
            password[i] = all[random.Next(all.Length)];
        }

        return Ok(new string(password.OrderBy(_ => random.Next()).ToArray()));
    }
}
