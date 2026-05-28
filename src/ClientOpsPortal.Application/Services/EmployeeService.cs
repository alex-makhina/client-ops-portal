using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using System.Linq.Expressions;

namespace ClientOpsPortal.Application.Services
{
    public class EmployeeService : IEmployeeService
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

        public EmployeeService(
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

        public async Task<EmployeeDto?> GetByIdAsync(Guid id, bool withIncludes = false, CancellationToken ct = default)
        {
            var employee = await _employeeRepository.GetByIdAsync(id, withIncludes, ct);
            return employee?.ToEmployeeDto();
        }

        public async Task<IReadOnlyCollection<EmployeeDto>> GetAllAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var employees = await _employeeRepository.GetAllAsync(withIncludes, ct);
            return employees.Select(e => e.ToEmployeeDto()).ToList();
        }

        public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto createDto, CancellationToken ct = default)
        {
            var existingEmployee = await GetEmployeeByStaffNumberAsync(createDto.StaffNumber, ct);
            if (existingEmployee != null)
                throw new InvalidOperationException($"Сотрудник с табельным номером {createDto.StaffNumber} уже существует");

            var userName = createDto.StaffNumber;
            var password = _identityService.GenerateRandomPassword();

            var user = await _identityService.CreateUserAsync(userName, createDto.Email, password, "Employee", ct);

            var employee = createDto.ToEntity();
            employee.UserId = user.Id;
            await _employeeRepository.AddAsync(employee, ct);

            var resetToken = await _identityService.GeneratePasswordResetTokenAsync(userName, ct);
            var resetLink = $"http://localhost:5022/set-password?userId={user.ExternalId}&token={Uri.EscapeDataString(resetToken)}";
            await _notificationService.SendPasswordSetLinkAsync(createDto.Email, userName, resetLink, ct);

            return employee.ToEmployeeDto();
        }

        public async Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeDto updateDto, CancellationToken ct = default)
        {
            var employee = await _employeeRepository.GetByIdAsync(id, false, ct);
            if (employee == null)
                throw new EntityNotFoundException(typeof(Employee), id);

            if (!string.IsNullOrWhiteSpace(updateDto.StaffNumber) && employee.StaffNumber != updateDto.StaffNumber)
            {
                var existingEmployee = await GetEmployeeByStaffNumberAsync(updateDto.StaffNumber, ct);
                if (existingEmployee != null)
                    throw new InvalidOperationException($"Сотрудник с табельным номером {updateDto.StaffNumber} уже существует");
            }

            updateDto.UpdateEntity(employee);
            await _employeeRepository.UpdateAsync(employee, ct);
            return employee.ToEmployeeDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _employeeRepository.DeleteAsync(id, ct);
        }

        public async Task<IReadOnlyCollection<EmployeeDto>> GetWhereAsync(Expression<Func<Employee, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
        {
            var employees = await _employeeRepository.GetWhereAsync(predicate, withIncludes, ct);
            return employees.Select(e => e.ToEmployeeDto()).ToList();
        }

        public async Task<EmployeeShortDataDto?> GetEmployeeByStaffNumberAsync(string staffNumber, CancellationToken ct = default)
        {
            var employee = (await _employeeRepository.GetWhereAsync(e => e.StaffNumber == staffNumber, true, ct)).FirstOrDefault();
            return employee?.ToEmployeeShortDataDto();
        }

        public async Task<IReadOnlyCollection<EmployeeShortDataDto>> SearchByFullNameAsync(string searchTerm, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<EmployeeShortDataDto>();

            searchTerm = searchTerm.Trim().ToLower();

            var employees = await _employeeRepository.GetWhereAsync(e =>
                e.FirstName.ToLower().Contains(searchTerm) ||
                e.LastName.ToLower().Contains(searchTerm) ||
                (e.MiddleName != null && e.MiddleName.ToLower().Contains(searchTerm)) ||
                (e.FirstName.ToLower() + " " + e.LastName.ToLower()).Contains(searchTerm) ||
                (e.LastName.ToLower() + " " + e.FirstName.ToLower()).Contains(searchTerm),
                false, ct);

            return employees.Select(e => e.ToEmployeeShortDataDto()).ToList();
        }

        public async Task<IReadOnlyCollection<EmployeeShortDataDto>> GetEmployeesByPostAsync(string post, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(post))
                return new List<EmployeeShortDataDto>();

            var employees = await _employeeRepository.GetWhereAsync(
                e => e.Post.ToLower().Contains(post.ToLower()),
                false, ct);

            return employees.Select(e => e.ToEmployeeShortDataDto()).ToList();
        }

        public async Task<IReadOnlyCollection<EmployeeShortDataDto>> GetEmployeesByDepartmentAsync(string department, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(department))
                return new List<EmployeeShortDataDto>();

            var employees = await _employeeRepository.GetWhereAsync(
                e => e.Department != null && e.Department.ToLower().Contains(department.ToLower()),
                false, ct);

            return employees.Select(e => e.ToEmployeeShortDataDto()).ToList();
        }

        // --- Admin user management methods ---

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

        /// <summary>
        /// Creates an employee with full admin user management (supports role mapping, custom login, password).
        /// </summary>
        public async Task<EmployeeDto> CreateAdminUserAsync(CreateEmployeeDto createDto, CancellationToken ct = default)
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

            var resetToken = await _identityService.GeneratePasswordResetTokenAsync(createDto.Login, ct);
            var resetLink = $"http://localhost:5022/set-password?userId={user.ExternalId}&token={Uri.EscapeDataString(resetToken)}";
            await _notificationService.SendPasswordSetLinkAsync(createDto.Email, createDto.Login, resetLink, ct);

            var dto = employee.ToEmployeeDto();
            dto.Login = createDto.Login;
            dto.Role = createDto.Role;
            return dto;
        }

        /// <summary>
        /// Updates an employee with full admin user management (supports role update in Identity).
        /// </summary>
        public async Task<EmployeeDto> UpdateAdminUserAsync(Guid employeeId, UpdateEmployeeDto updateDto, CancellationToken ct = default)
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

    }
}
