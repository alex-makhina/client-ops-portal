using ClientOpsPortal.Web.Features.UserManagement.Models;

namespace ClientOpsPortal.Web.Features.UserManagement.Services
{
    public interface IUserManagementService
    {
        Task<List<UserListItem>> GetAllUsersAsync();
        Task<(bool success, string errorMessage)> CreateUserAsync(CreateUserRequest request);
        Task<(bool success, string errorMessage)> UpdateUserAsync(Guid employeeId, UpdateUserRequest request);
        Task<bool> ToggleUserStatusAsync(Guid employeeId);
        Task<bool> DeleteUserAsync(Guid employeeId);
        Task<List<string>> GetAvailableRolesAsync();
    }
}
