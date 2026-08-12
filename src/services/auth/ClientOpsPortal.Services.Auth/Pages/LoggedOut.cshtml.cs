using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClientOpsPortal.Services.Auth.Pages;

public class LoggedOutModel : PageModel
{
    public string? ReturnUrl { get; set; }

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
        ReturnUrl = returnUrl;
    }
}
