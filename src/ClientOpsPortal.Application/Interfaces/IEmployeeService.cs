using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface IEmployeeService : IBaseService<Employee, EmployeeDto, CreateEmployeeDto, UpdateEmployeeDto>
    {
        Task<EmployeeShortDataDto?> GetEmployeeByStaffNumberAsync(string staffNumber, CancellationToken ct = default);
        Task<IReadOnlyCollection<EmployeeShortDataDto>> SearchByFullNameAsync(string searchTerm, CancellationToken ct = default);
        Task<IReadOnlyCollection<EmployeeShortDataDto>> GetEmployeesByPostAsync(string post, CancellationToken ct = default);
        Task<IReadOnlyCollection<EmployeeShortDataDto>> GetEmployeesByDepartmentAsync(string department, CancellationToken ct = default);
        
    }
}