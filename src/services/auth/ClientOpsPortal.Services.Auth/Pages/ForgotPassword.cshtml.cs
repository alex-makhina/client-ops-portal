using ClientOpsPortal.Services.Auth.Domain;
using ClientOpsPortal.Services.Notifications.Client;
using ClientOpsPortal.Services.Notifications.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ClientOpsPortal.Services.Auth.Pages;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(
        UserManager<ApplicationUser> userManager,
        INotificationPublisher notificationPublisher,
        ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    [BindProperty]
    public string LoginIdentifier { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public string LoginUrl
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ReturnUrl))
                return "/Login";

            var separator = ReturnUrl.Contains('?') ? '&' : '?';
            return $"/Login{separator}returnUrl={Uri.EscapeDataString(ReturnUrl)}";
        }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = IsAllowedReturnUrl(returnUrl) ? returnUrl : null;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = IsAllowedReturnUrl(returnUrl) ? returnUrl : null;

        if (string.IsNullOrWhiteSpace(LoginIdentifier))
        {
            ErrorMessage = "Введите логин.";
            return Page();
        }

        var user = await _userManager.FindByNameAsync(LoginIdentifier.Trim());
        if (user is null)
        {
            _logger.LogWarning("Password reset requested for unknown user {LoginIdentifier}", LoginIdentifier.Trim());
            ErrorMessage = "Пользователь с таким логином не найден.";
            return Page();
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{Request.Scheme}://{Request.Host}/ResetPassword?userId={user.Id}&token={Uri.EscapeDataString(resetToken)}";
        if (ReturnUrl is not null)
            resetLink += $"&returnUrl={Uri.EscapeDataString(ReturnUrl)}";

        await _notificationPublisher.PublishAsync(new NotificationMessage
        {
            Type = NotificationType.PasswordResetLink,
            RecipientEmail = user.Email ?? string.Empty,
            Login = LoginIdentifier.Trim(),
            ResetLink = resetLink
        });

        _logger.LogInformation("Password reset link sent to user {UserId} ({LoginIdentifier})", user.Id, LoginIdentifier.Trim());

        SuccessMessage = "Ссылка для восстановления пароля отправлена на вашу почту.";
        return Page();
    }

    private static bool IsAllowedReturnUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        return uri.Host is "localhost" or "127.0.0.1" &&
               uri.Port is 5022 or 62000;
    }
}
