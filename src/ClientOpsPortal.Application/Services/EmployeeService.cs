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

        public EmployeeService(IGenericRepository<Employee> employeeRepository)
        {
            _employeeRepository = employeeRepository;
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

            var employee = createDto.ToEntity();
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
    }
}