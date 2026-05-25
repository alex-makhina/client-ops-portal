using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;

namespace ClientOpsPortal.Application.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IGenericRepository<Employee> _employeeRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IIdentityService _identityService;
        private readonly INotificationService _notificationService;

        private static readonly Dictionary<string, string> RoleDisplayToBackend = new()
        {
            { "Администратор", "Admin" },
            { "Специалист по услугам", "ServiceManager" },
            { "Аналитик", "DataAnalyst" },
            { "Специалист по работе с клиентами", "Manager" }
        };

        private static readonly Dictionary<string, string> RoleBackendToDisplay = new()
        {
            { "Admin", "Администратор" },
            { "ServiceManager", "Специалист по услугам" },
            { "DataAnalyst", "Аналитик" },
            { "Manager", "Специалист по работе с клиентами" }
        };

        public UserManagementService(
            IGenericRepository<Employee> employeeRepository,
            IGenericRepository<User> userRepository,
            IIdentityService identityService,
            INotificationService notificationService)
        {
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _identityService = identityService;
            _notificationService = notificationService;
        }

        public async Task<IReadOnlyCollection<UserListItemDto>> GetAllUsersAsync(CancellationToken ct = default)
        {
            var employees = await _employeeRepository.GetWhereAsync(e => true, true, ct);
            var result = new List<UserListItemDto>();

            foreach (var employee in employees)
            {
                if (employee.User == null)
                    continue;

                var appUser = await _identityService.FindApplicationUserByExternalIdAsync(employee.User.ExternalId, ct);
                if (appUser == null)
                    continue;

                var roles = await _identityService.GetUserRolesAsync(appUser.Id, ct);
                var role = roles.FirstOrDefault() ?? "Unknown";
                var displayRole = RoleBackendToDisplay.GetValueOrDefault(role, role);

                result.Add(employee.ToUserListItemDto(appUser.UserName ?? string.Empty, displayRole));
            }

            return result.OrderBy(r => r.FullName).ToList();
        }

        public async Task<EmployeeDto> CreateUserAsync(CreateEmployeeDto createDto, CancellationToken ct = default)
        {
            var existingEmployee = (await _employeeRepository.GetWhereAsync(
                e => e.StaffNumber == createDto.StaffNumber, false, ct)).FirstOrDefault();
            if (existingEmployee != null)
                throw new InvalidOperationException($"Сотрудник с табельным номером {createDto.StaffNumber} уже существует");

            var backendRole = RoleDisplayToBackend.GetValueOrDefault(createDto.Role, createDto.Role);
            var password = string.IsNullOrWhiteSpace(createDto.Password)
                ? _identityService.GenerateRandomPassword()
                : createDto.Password;

            var user = await _identityService.CreateUserAsync(createDto.Login, createDto.Email, password, backendRole, ct);

            var employee = createDto.ToEntity();
            employee.UserId = user.Id;
            await _employeeRepository.AddAsync(employee, ct);

            await _notificationService.SendWelcomeWithPasswordAsync(createDto.Email, createDto.Login, password, ct);

            var dto = employee.ToEmployeeDto();
            dto.Login = createDto.Login;
            dto.Role = createDto.Role;
            return dto;
        }

        public async Task<EmployeeDto> UpdateUserAsync(Guid employeeId, UpdateEmployeeDto updateDto, CancellationToken ct = default)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId, true, ct);
            if (employee == null)
                throw new EntityNotFoundException(typeof(Employee), employeeId);

            if (!string.IsNullOrWhiteSpace(updateDto.StaffNumber) && employee.StaffNumber != updateDto.StaffNumber)
            {
                var existingEmployee = (await _employeeRepository.GetWhereAsync(
                    e => e.StaffNumber == updateDto.StaffNumber, false, ct)).FirstOrDefault();
                if (existingEmployee != null)
                    throw new InvalidOperationException($"Сотрудник с табельным номером {updateDto.StaffNumber} уже существует");
            }

            updateDto.UpdateEntity(employee);
            await _employeeRepository.UpdateAsync(employee, ct);

            // Update role in Identity
            if (employee.User != null)
            {
                var appUserId = Guid.Parse(employee.User.ExternalId);
                var backendRole = RoleDisplayToBackend.GetValueOrDefault(updateDto.Role, updateDto.Role);
                await _identityService.SetUserRoleAsync(appUserId, backendRole, ct);
            }

            var dto = employee.ToEmployeeDto();
            dto.Role = updateDto.Role;
            return dto;
        }

        public async Task ToggleUserStatusAsync(Guid employeeId, CancellationToken ct = default)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId, true, ct);
            if (employee == null)
                throw new EntityNotFoundException(typeof(Employee), employeeId);

            employee.IsActive = !employee.IsActive;
            await _employeeRepository.UpdateAsync(employee, ct);

            if (employee.User != null)
            {
                var appUserId = Guid.Parse(employee.User.ExternalId);
                if (employee.IsActive)
                {
                    await _identityService.UnblockUserAsync(appUserId, ct);
                }
                else
                {
                    await _identityService.BlockUserAsync(appUserId, ct);
                }
            }
        }

        public async Task DeleteUserAsync(Guid employeeId, CancellationToken ct = default)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId, true, ct);
            if (employee == null)
                throw new EntityNotFoundException(typeof(Employee), employeeId);

            await _employeeRepository.DeleteAsync(employeeId, ct);
        }

        public Task<IReadOnlyCollection<string>> GetAvailableRolesAsync(CancellationToken ct = default)
        {
            var roles = new List<string>
            {
                "Администратор",
                "Специалист по услугам",
                "Аналитик",
                "Специалист по работе с клиентами"
            };

            return Task.FromResult<IReadOnlyCollection<string>>(roles);
        }
    }
}
