using ClientOpsPortal.Services.Auth.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClientOpsPortal.Services.Auth.Pages;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
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
            ErrorMessage = "Неверный логин или пароль.";
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Password, RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ErrorMessage = result.IsLockedOut
                ? "Пользователь заблокирован. Попробуйте позже."
                : "Неверный логин или пароль.";
            return Page();
        }

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
