using ClientOpsPortal.Services.Auth.Client;
using ClientOpsPortal.Services.Auth.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Api.Controllers;

/// <summary>
/// BFF proxy — forwards auth requests to the dedicated auth microservice.
/// Keeps the frontend URLs unchanged (they call the main API on :5079).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthClient _authClient;

    public AuthController(IAuthClient authClient) => _authClient = authClient;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _authClient.LoginAsync(request, ct);
            return Ok(response);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return Unauthorized("Invalid username or password");
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var token = await _authClient.GeneratePasswordResetTokenAsync(request.LoginIdentifier, ct);
        return Ok(new ForgotPasswordResponse { TemporaryPassword = token });
    }

    [HttpPost("set-password")]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request, CancellationToken ct = default)
    {
        try
        {
            await _authClient.SetPasswordAsync(request, ct);
            return Ok(new { message = "Password has been set successfully." });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.BadRequest), "Set password failed");
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct = default)
    {
        try
        {
            await _authClient.ResetPasswordAsync(request, ct);
            return Ok(new { message = "Password has been changed successfully." });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.BadRequest), "Reset password failed");
        }
    }
}