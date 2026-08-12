namespace ClientOpsPortal.Web.Features.Auth.Services
{
    public interface IAuthService
    {
        void Login();
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
    }
}
