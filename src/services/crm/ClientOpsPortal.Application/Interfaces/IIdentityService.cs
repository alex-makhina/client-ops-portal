using ClientOpsPortal.Domain.Entities;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<User> CreateUserAsync(string userName, string email, string password, string role, CancellationToken ct = default);
        string GenerateRandomPassword();
        Task<string> ResetPasswordAsync(string userName, CancellationToken ct = default);
        Task<string> GeneratePasswordResetTokenAsync(string userName, CancellationToken ct = default);
        Task BlockUserAsync(Guid applicationUserId, CancellationToken ct = default);
        Task UnblockUserAsync(Guid applicationUserId, CancellationToken ct = default);
        Task<IList<string>> GetUserRolesAsync(Guid applicationUserId, CancellationToken ct = default);
        Task SetUserRoleAsync(Guid applicationUserId, string role, CancellationToken ct = default);
        Task<ApplicationUser?> FindApplicationUserByExternalIdAsync(string externalId, CancellationToken ct = default);
    }
}
