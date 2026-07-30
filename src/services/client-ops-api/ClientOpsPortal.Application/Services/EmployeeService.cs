using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using ClientOpsPortal.Services.Auth.Client;
using ClientOpsPortal.Services.Auth.Contracts;
using System.Linq.Expressions;

namespace ClientOpsPortal.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IGenericRepository<Employee> _employeeRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IAuthClient _authClient;

        private static readonly Dictionary<string, string> RoleDisplayToBackend = new()
        {
            { "Р С’Р Т‘Р СР С‘Р Р…Р С‘РЎРѓРЎвЂљРЎР‚Р В°РЎвЂљР С•РЎР‚", "Admin" },
            { "Р РЋР С—Р ВµРЎвЂ Р С‘Р В°Р В»Р С‘РЎРѓРЎвЂљ Р С—Р С• РЎС“РЎРѓР В»РЎС“Р С–Р В°Р С", "ServiceManager" },
            { "Р С’Р Р…Р В°Р В»Р С‘РЎвЂљР С‘Р С”", "DataAnalyst" },
            { "Р РЋР С—Р ВµРЎвЂ Р С‘Р В°Р В»Р С‘РЎРѓРЎвЂљ Р С—Р С• РЎР‚Р В°Р В±Р С•РЎвЂљР Вµ РЎРѓ Р С”Р В»Р С‘Р ВµР Р…РЎвЂљР В°Р СР С‘", "Manager" }
        };

        private static readonly Dictionary<string, string> RoleBackendToDisplay = new()
        {
            { "Admin", "Р С’Р Т‘Р СР С‘Р Р…Р С‘РЎРѓРЎвЂљРЎР‚Р В°РЎвЂљР С•РЎР‚" },
            { "ServiceManager", "Р РЋР С—Р ВµРЎвЂ Р С‘Р В°Р В»Р С‘РЎРѓРЎвЂљ Р С—Р С• РЎС“РЎРѓР В»РЎС“Р С–Р В°Р С" },
            { "DataAnalyst", "Р С’Р Р…Р В°Р В»Р С‘РЎвЂљР С‘Р С”" },
            { "Manager", "Р РЋР С—Р ВµРЎвЂ Р С‘Р В°Р В»Р С‘РЎРѓРЎвЂљ Р С—Р С• РЎР‚Р В°Р В±Р С•РЎвЂљР Вµ РЎРѓ Р С”Р В»Р С‘Р ВµР Р…РЎвЂљР В°Р СР С‘" }
        };

        public EmployeeService(
            IGenericRepository<Employee> employeeRepository,
            IGenericRepository<User> userRepository,
            IAuthClient authClient)
        {
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _authClient = authClient;
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
                throw new InvalidOperationException($"Р РЋР С•РЎвЂљРЎР‚РЎС“Р Т‘Р Р…Р С‘Р С” РЎРѓ РЎвЂљР В°Р В±Р ВµР В»РЎРЉР Р…РЎвЂ№Р С Р Р…Р С•Р СР ВµРЎР‚Р С•Р С {createDto.StaffNumber} РЎС“Р В¶Р Вµ РЎРѓРЎС“РЎвЂ°Р ВµРЎРѓРЎвЂљР Р†РЎС“Р ВµРЎвЂљ");

            var userName = createDto.StaffNumber;
            var password = await _authClient.GenerateRandomPasswordAsync(ct);
            var userId = await _authClient.CreateUserAsync(new CreateUserRequest
            {
                UserName = userName,
                Password = password,
                Email = createDto.Email,
                Roles = new List<string> { "Manager" }
            }, ct);

            var user = new User { Id = Guid.NewGuid(), ExternalId = userId };
            await _userRepository.AddAsync(user, ct);

            var employee = createDto.ToEntity();
            employee.UserId = user.Id;
            await _employeeRepository.AddAsync(employee, ct);

            return employee.ToEmployeeDto();
        }

        public async Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeDto updateDto, CancellationToken ct = default)
        {
            var employee = await _employeeRepository.GetByIdAsync(id, false, ct);
            if (employee == null)
                throw new EntityNotFoundException(typeof(Employee), id);
            if (!string.IsNullOrWhiteSpace(updateDto.StaffNumber) && employee.StaffNumber != updateDto.StaffNumber)
            {
                var existing = await GetEmployeeByStaffNumberAsync(updateDto.StaffNumber, ct);
                if (existing != null) throw new InvalidOperationException($"Р РЋР С•РЎвЂљРЎР‚РЎС“Р Т‘Р Р…Р С‘Р С” РЎРѓ РЎвЂљР В°Р В±Р ВµР В»РЎРЉР Р…РЎвЂ№Р С Р Р…Р С•Р СР ВµРЎР‚Р С•Р С {updateDto.StaffNumber} РЎС“Р В¶Р Вµ РЎРѓРЎС“РЎвЂ°Р ВµРЎРѓРЎвЂљР Р†РЎС“Р ВµРЎвЂљ");
            }
            updateDto.UpdateEntity(employee);
            await _employeeRepository.UpdateAsync(employee, ct);
            return employee.ToEmployeeDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default) => await _employeeRepository.DeleteAsync(id, ct);

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
            if (string.IsNullOrWhiteSpace(searchTerm)) return new List<EmployeeShortDataDto>();
            searchTerm = searchTerm.Trim().ToLower();
            var employees = await _employeeRepository.GetWhereAsync(e =>
                e.FirstName.ToLower().Contains(searchTerm) || e.LastName.ToLower().Contains(searchTerm), false, ct);
            return employees.Select(e => e.ToEmployeeShortDataDto()).ToList();
        }

        public async Task<IReadOnlyCollection<EmployeeShortDataDto>> GetEmployeesByPostAsync(string post, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(post)) return new List<EmployeeShortDataDto>();
            var employees = await _employeeRepository.GetWhereAsync(e => e.Post.ToLower().Contains(post.ToLower()), false, ct);
            return employees.Select(e => e.ToEmployeeShortDataDto()).ToList();
        }

        public async Task<IReadOnlyCollection<EmployeeShortDataDto>> GetEmployeesByDepartmentAsync(string department, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(department)) return new List<EmployeeShortDataDto>();
            var employees = await _employeeRepository.GetWhereAsync(e => e.Department != null && e.Department.ToLower().Contains(department.ToLower()), false, ct);
            return employees.Select(e => e.ToEmployeeShortDataDto()).ToList();
        }

        // --- Admin user management methods ---

        public async Task<IReadOnlyCollection<UserListItemDto>> GetAllUsersAsync(CancellationToken ct = default)
        {
            var employees = await _employeeRepository.GetWhereAsync(e => true, true, ct);
            var result = new List<UserListItemDto>();
            foreach (var employee in employees)
            {
                if (employee.User == null) continue;
                UserResponse? appUser;
                try { appUser = await _authClient.GetUserByIdAsync(employee.User.ExternalId, ct); }
                catch { continue; }
                if (appUser == null) continue;
                var role = appUser.Roles.FirstOrDefault() ?? "Unknown";
                var displayRole = RoleBackendToDisplay.GetValueOrDefault(role, role);
                result.Add(employee.ToUserListItemDto(appUser.UserName, displayRole));
            }
            return result.OrderBy(r => r.FullName).ToList();
        }

        public async Task ToggleUserStatusAsync(Guid employeeId, CancellationToken ct = default)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId, true, ct);
            if (employee == null) throw new EntityNotFoundException(typeof(Employee), employeeId);
            employee.IsActive = !employee.IsActive;
            await _employeeRepository.UpdateAsync(employee, ct);
            if (employee.User != null)
            {
                if (employee.IsActive) await _authClient.UnblockUserAsync(employee.User.ExternalId, ct);
                else await _authClient.BlockUserAsync(employee.User.ExternalId, ct);
            }
        }

        public Task<IReadOnlyCollection<string>> GetAvailableRolesAsync(CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyCollection<string>>(new List<string>
            {
                "Р С’Р Т‘Р СР С‘Р Р…Р С‘РЎРѓРЎвЂљРЎР‚Р В°РЎвЂљР С•РЎР‚", "Р РЋР С—Р ВµРЎвЂ Р С‘Р В°Р В»Р С‘РЎРѓРЎвЂљ Р С—Р С• РЎС“РЎРѓР В»РЎС“Р С–Р В°Р С", "Р С’Р Р…Р В°Р В»Р С‘РЎвЂљР С‘Р С”", "Р РЋР С—Р ВµРЎвЂ Р С‘Р В°Р В»Р С‘РЎРѓРЎвЂљ Р С—Р С• РЎР‚Р В°Р В±Р С•РЎвЂљР Вµ РЎРѓ Р С”Р В»Р С‘Р ВµР Р…РЎвЂљР В°Р СР С‘"
            });
        }

        public async Task<EmployeeDto> CreateAdminUserAsync(CreateEmployeeDto createDto, CancellationToken ct = default)
        {
            var existing = (await _employeeRepository.GetWhereAsync(e => e.StaffNumber == createDto.StaffNumber, false, ct)).FirstOrDefault();
            if (existing != null) throw new InvalidOperationException($"Р РЋР С•РЎвЂљРЎР‚РЎС“Р Т‘Р Р…Р С‘Р С” РЎРѓ РЎвЂљР В°Р В±Р ВµР В»РЎРЉР Р…РЎвЂ№Р С Р Р…Р С•Р СР ВµРЎР‚Р С•Р С {createDto.StaffNumber} РЎС“Р В¶Р Вµ РЎРѓРЎС“РЎвЂ°Р ВµРЎРѓРЎвЂљР Р†РЎС“Р ВµРЎвЂљ");

            var backendRole = RoleDisplayToBackend.GetValueOrDefault(createDto.Role, createDto.Role);
            var password = string.IsNullOrWhiteSpace(createDto.Password)
                ? await _authClient.GenerateRandomPasswordAsync(ct)
                : createDto.Password;

            var userId = await _authClient.CreateUserAsync(new CreateUserRequest
            {
                UserName = createDto.Login,
                Password = password,
                Email = createDto.Email,
                Roles = new List<string> { backendRole }
            }, ct);

            var user = new User { Id = Guid.NewGuid(), ExternalId = userId };
            await _userRepository.AddAsync(user, ct);

            var employee = createDto.ToEntity();
            employee.UserId = user.Id;
            await _employeeRepository.AddAsync(employee, ct);

            var dto = employee.ToEmployeeDto();
            dto.Login = createDto.Login;
            dto.Role = createDto.Role;
            return dto;
        }

        public async Task<EmployeeDto> UpdateAdminUserAsync(Guid employeeId, UpdateEmployeeDto updateDto, CancellationToken ct = default)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId, true, ct);
            if (employee == null) throw new EntityNotFoundException(typeof(Employee), employeeId);
            if (!string.IsNullOrWhiteSpace(updateDto.StaffNumber) && employee.StaffNumber != updateDto.StaffNumber)
            {
                var existing = (await _employeeRepository.GetWhereAsync(e => e.StaffNumber == updateDto.StaffNumber, false, ct)).FirstOrDefault();
                if (existing != null) throw new InvalidOperationException($"Р РЋР С•РЎвЂљРЎР‚РЎС“Р Т‘Р Р…Р С‘Р С” РЎРѓ РЎвЂљР В°Р В±Р ВµР В»РЎРЉР Р…РЎвЂ№Р С Р Р…Р С•Р СР ВµРЎР‚Р С•Р С {updateDto.StaffNumber} РЎС“Р В¶Р Вµ РЎРѓРЎС“РЎвЂ°Р ВµРЎРѓРЎвЂљР Р†РЎС“Р ВµРЎвЂљ");
            }
            updateDto.UpdateEntity(employee);
            await _employeeRepository.UpdateAsync(employee, ct);
            if (employee.User != null && !string.IsNullOrWhiteSpace(updateDto.Role))
            {
                var backendRole = RoleDisplayToBackend.GetValueOrDefault(updateDto.Role, updateDto.Role);
                await _authClient.SetUserRoleAsync(new SetUserRoleRequest { UserId = employee.User.ExternalId, Role = backendRole }, ct);
            }
            var dto = employee.ToEmployeeDto();
            dto.Role = updateDto.Role;
            return dto;
        }
    }
}