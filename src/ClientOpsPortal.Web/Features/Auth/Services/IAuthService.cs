using ClientOpsPortal.Web.Features.Auth.Models;

namespace ClientOpsPortal.Web.Features.Auth.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(string loginIdentifier, string password);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<HttpResponseMessage> SetPassword(Guid userId, string token, string newPass);
    }
}
