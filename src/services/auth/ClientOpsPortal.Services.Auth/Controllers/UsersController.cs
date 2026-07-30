using ClientOpsPortal.Services.Auth.Contracts;
using ClientOpsPortal.Services.Auth.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Services.Auth.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

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
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        return Ok(new UserResponse
        {
            Id = user.Id.ToString(), UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty, IsBlocked = await _userManager.IsLockedOutAsync(user), Roles = roles
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var user = new ApplicationUser { UserName = request.UserName, Email = request.Email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
        foreach (var role in request.Roles)
            if (await _roleManager.RoleExistsAsync(role))
                await _userManager.AddToRoleAsync(user, role);
        return Ok(user.Id.ToString());
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> SetRole(Guid id, [FromBody] SetUserRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (await _roleManager.RoleExistsAsync(request.Role))
            await _userManager.AddToRoleAsync(user, request.Role);
        return Ok();
    }

    [HttpPost("{id}/block")]
    public async Task<IActionResult> Block(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        return Ok();
    }

    [HttpPost("{id}/unblock")]
    public async Task<IActionResult> Unblock(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();
        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        return Ok();
    }

    [HttpGet("random-password")]
    public IActionResult GenerateRandomPassword()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+";
        var random = new Random();
        var password = new string(Enumerable.Range(0, 12).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        return Ok(password);
    }
}