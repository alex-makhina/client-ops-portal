using ClientOpsPortal.Application.DTOs;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface IUserManagementService
    {
        Task<IReadOnlyCollection<UserListItemDto>> GetAllUsersAsync(CancellationToken ct = default);
        Task<EmployeeDto> CreateUserAsync(CreateEmployeeDto createDto, CancellationToken ct = default);
        Task<EmployeeDto> UpdateUserAsync(Guid employeeId, UpdateEmployeeDto updateDto, CancellationToken ct = default);
        Task ToggleUserStatusAsync(Guid employeeId, CancellationToken ct = default);
        Task DeleteUserAsync(Guid employeeId, CancellationToken ct = default);
        Task<IReadOnlyCollection<string>> GetAvailableRolesAsync(CancellationToken ct = default);
    }
}
