using ClientOpsPortal.Services.Auth.Contracts;
using ClientOpsPortal.Services.Auth.Domain;
using ClientOpsPortal.Services.Auth.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClientOpsPortal.Services.Auth.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOptions<JwtSettings> _jwtSettings;

    public AuthController(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Login);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Invalid username or password");

        if (await _userManager.IsLockedOutAsync(user))
            return Unauthorized("User is blocked");

        var token = await GenerateJwtTokenAsync(user);
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        return Ok(new AuthResponse
        {
            Token = token,
            Roles = roles,
            UserId = user.Id.ToString(),
            UserName = user.UserName ?? string.Empty
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.LoginIdentifier);
        if (user == null) return NotFound("User not found");
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        return Ok(new ForgotPasswordResponse { TemporaryPassword = resetToken });
    }

    [HttpPost("set-password")]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.LoginIdentifier);
        if (user == null) return NotFound("User not found");
        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return Ok(new { message = "Password has been set successfully." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.LoginIdentifier);
        if (user == null) return NotFound("User not found");
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return Ok(new { message = "Password has been changed successfully." });
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var settings = _jwtSettings.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty)
        };
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}