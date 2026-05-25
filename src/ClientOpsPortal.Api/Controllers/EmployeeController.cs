using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Api.Controllers
{
    [Authorize]
    public class EmployeesController : BaseController
    {
        private readonly IEmployeeService _employeeService;
        private readonly IUserManagementService _userManagementService;

        public EmployeesController(
            IEmployeeService employeeService,
            IUserManagementService userManagementService,
            ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _employeeService = employeeService;
            _userManagementService = userManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var employees = await _employeeService.GetAllAsync(withIncludes, ct);
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var employee = await _employeeService.GetByIdAsync(id, withIncludes, ct);
            if (employee == null)
                return NotFound($"Сотрудник с ID {id} не найден");
            return Ok(employee);
        }

        [HttpGet("by-staff-number/{staffNumber}")]
        public async Task<IActionResult> GetByStaffNumber(string staffNumber, CancellationToken ct = default)
        {
            var employee = await _employeeService.GetEmployeeByStaffNumberAsync(staffNumber, ct);
            if (employee == null)
                return NotFound($"Сотрудник с табельным номером {staffNumber} не найден");
            return Ok(employee);
        }

        [HttpGet("search/by-name")]
        public async Task<IActionResult> SearchByName([FromQuery] string searchTerm, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term cannot be empty");

            var result = await _employeeService.SearchByFullNameAsync(searchTerm, ct);
            return Ok(result);
        }

        [HttpGet("by-post")]
        public async Task<IActionResult> GetByPost([FromQuery] string post, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(post))
                return BadRequest("Post cannot be empty");

            var employees = await _employeeService.GetEmployeesByPostAsync(post, ct);
            return Ok(employees);
        }

        [HttpGet("by-department")]
        public async Task<IActionResult> GetByDepartment([FromQuery] string department, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(department))
                return BadRequest("Department cannot be empty");

            var employees = await _employeeService.GetEmployeesByDepartmentAsync(department, ct);
            return Ok(employees);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateEmployeeDto createDto, CancellationToken ct = default)
        {
            try
            {
                var employee = await _employeeService.CreateAsync(createDto, ct);
                return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateEmployeeDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var employee = await _employeeService.UpdateAsync(id, updateDto, ct);
                return Ok(employee);
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Сотрудник с ID {id} не найден");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _employeeService.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Сотрудник с ID {id} не найден");
            }
        }

        [HttpGet("admin/list")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminUserList(CancellationToken ct = default)
        {
            var users = await _userManagementService.GetAllUsersAsync(ct);
            return Ok(users);
        }

        [HttpPost("admin/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAdminUser([FromBody] CreateEmployeeDto createDto, CancellationToken ct = default)
        {
            try
            {
                var employee = await _userManagementService.CreateUserAsync(createDto, ct);
                return CreatedAtAction(nameof(GetAdminUserList), null, employee);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAdminUser(Guid id, [FromBody] UpdateEmployeeDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var employee = await _userManagementService.UpdateUserAsync(id, updateDto, ct);
                return Ok(employee);
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Сотрудник с ID {id} не найден");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("admin/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _userManagementService.ToggleUserStatusAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Сотрудник с ID {id} не найден");
            }
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAdminUser(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _userManagementService.DeleteUserAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Сотрудник с ID {id} не найден");
            }
        }

        [HttpGet("admin/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAvailableRoles(CancellationToken ct = default)
        {
            var roles = await _userManagementService.GetAvailableRolesAsync(ct);
            return Ok(roles);
        }
    }
}
