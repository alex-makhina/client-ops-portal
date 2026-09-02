using ClientOpsPortal.Services.Auth.Domain;
using ClientOpsPortal.Services.Auth.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ClientOpsPortal.Services.Auth.Pages;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthSettings _authSettings;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        AuthSettings authSettings,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _authSettings = authSettings;
        _logger = logger;
    }

    [BindProperty]
    public string UserName { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? "/";
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? "/";

        var user = await _userManager.FindByNameAsync(UserName);
        if (user is null)
        {
            _logger.LogWarning("Login failed for {UserName}: user not found", UserName);
            ErrorMessage = "Неверный логин или пароль.";
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Password, RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Login failed for user {UserId} ({UserName}): account is locked out", user.Id, UserName);
            }
            else
            {
                _logger.LogWarning("Login failed for user {UserId} ({UserName}): invalid password", user.Id, UserName);
            }

            ErrorMessage = result.IsLockedOut
                ? "Пользователь заблокирован. Попробуйте позже."
                : "Неверный логин или пароль.";
            return Page();
        }

        _logger.LogInformation("User {UserId} ({UserName}) logged in", user.Id, UserName);

        var roles = await _userManager.GetRolesAsync(user);
        var targetUrl = _authSettings.IsAllowedReturnUrl(ReturnUrl)
            ? ReturnUrl
            : $"{_authSettings.GetPortalUrl(roles)}/";

        return Redirect(targetUrl);
    }
}
