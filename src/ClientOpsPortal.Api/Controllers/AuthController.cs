using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Settings;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClientOpsPortal.Api.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IIdentityService _identityService;
        private readonly INotificationService _notificationService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtSettings,
            IIdentityService identityService,
            INotificationService notificationService,
            ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _identityService = identityService;
            _notificationService = notificationService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Login);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Unauthorized("Invalid username or password");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            return Ok(new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.LoginIdentifier);
            if (user == null)
                return NotFound("User not found");

            var resetToken = await _identityService.GeneratePasswordResetTokenAsync(request.LoginIdentifier, CancellationToken.None);
            var resetLink = $"http://localhost:62000/set-password?userId={user.Id}&token={Uri.EscapeDataString(resetToken)}";
            await _notificationService.SendPasswordSetLinkAsync(user.Email ?? string.Empty, user.UserName ?? string.Empty, resetLink, CancellationToken.None);

            return Ok(new { message = "Password reset link has been sent to your email." });
        }

        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return NotFound("User not found");

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok(new { message = "Password set successfully" });
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public required string Login { get; set; }
        public required string Password { get; set; }
    }

    public class AuthResponse
    {
        public required string Token { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class ForgotPasswordRequest
    {
        public required string LoginIdentifier { get; set; }
    }

    public class ForgotPasswordResponse
    {
        public string TemporaryPassword { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public required string LoginIdentifier { get; set; }
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }

    public class SetPasswordRequest
    {
        public required Guid UserId { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }
}
