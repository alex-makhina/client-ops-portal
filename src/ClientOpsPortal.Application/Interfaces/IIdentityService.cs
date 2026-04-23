using ClientOpsPortal.Domain.Entities;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<User> CreateUserAsync(string userName, string email, string password, string role, CancellationToken ct = default);
        string GenerateRandomPassword();
        Task<string> ResetPasswordAsync(string userName, CancellationToken ct = default);
    }
}
