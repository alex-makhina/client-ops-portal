using ClientOpsPortal.Services.Auth.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ClientOpsPortal.Services.Auth.Pages;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ResetPasswordModel> _logger;

    public ResetPasswordModel(UserManager<ApplicationUser> userManager, ILogger<ResetPasswordModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? UserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Token { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public string LoginUrl => IsAllowedReturnUrl(ReturnUrl) ? ReturnUrl! : "/Login";

    private static bool IsAllowedReturnUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        return uri.Host is "localhost" or "127.0.0.1" &&
               uri.Port is 5022 or 62000;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(Token))
        {
            _logger.LogWarning("Reset password page opened with invalid link");
            ErrorMessage = "Ссылка для сброса пароля некорректна. Запросите новую ссылку.";
            return Page();
        }

        var user = await _userManager.FindByIdAsync(UserId);
        if (user is null)
        {
            _logger.LogWarning("Reset password requested for unknown user {UserId}", UserId);
            ErrorMessage = "Пользователь не найден. Запросите новую ссылку.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(Token))
        {
            ErrorMessage = "Ссылка для сброса пароля некорректна. Запросите новую ссылку.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "Введите новый пароль.";
            return Page();
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают.";
            return Page();
        }

        var user = await _userManager.FindByIdAsync(UserId);
        if (user is null)
        {
            ErrorMessage = "Пользователь не найден. Запросите новую ссылку.";
            return Page();
        }

        var result = await _userManager.ResetPasswordAsync(user, Token, NewPassword);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, string.Join("; ", result.Errors.Select(e => e.Description)));
            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            return Page();
        }

        _logger.LogInformation("Password reset completed for user {UserId}", user.Id);
        SuccessMessage = "Пароль успешно установлен. Теперь вы можете войти в систему.";
        return Page();
    }
}
