using ClientOpsPortal.Services.Auth.Domain;
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
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
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

        if (IsAllowedReturnUrl(ReturnUrl))
            return Redirect(ReturnUrl);

        return Redirect(await DefaultReturnUrlAsync(user));
    }

    private static bool IsAllowedReturnUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (url.StartsWith('/') && !url.StartsWith("//"))
            return true;

        if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
            return absolute.Host is "localhost" or "127.0.0.1" && absolute.Port is 5022 or 62000;

        return false;
    }

    private async Task<string> DefaultReturnUrlAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return roles.Contains("Abonent")
            ? "http://localhost:62000/"
            : "http://localhost:5022/";
    }
}
