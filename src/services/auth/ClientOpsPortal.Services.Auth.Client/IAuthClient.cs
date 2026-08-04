using ClientOpsPortal.Services.Auth.Contracts;

namespace ClientOpsPortal.Services.Auth.Client;

public interface IAuthClient
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<string> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserResponse> GetUserByIdAsync(string userId, CancellationToken ct = default);
    Task<List<UserResponse>> GetAllUsersAsync(CancellationToken ct = default);
    Task SetUserRoleAsync(SetUserRoleRequest request, CancellationToken ct = default);
    Task BlockUserAsync(string userId, CancellationToken ct = default);
    Task UnblockUserAsync(string userId, CancellationToken ct = default);
    Task<string> GeneratePasswordResetTokenAsync(string loginIdentifier, CancellationToken ct = default);
    Task<string> GenerateRandomPasswordAsync(CancellationToken ct = default);
    Task SetPasswordAsync(SetPasswordRequest request, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
}
