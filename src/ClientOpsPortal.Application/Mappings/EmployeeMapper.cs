using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Application.Mappings
{
    public static class EmployeeMapper
    {
        public static EmployeeDto ToEmployeeDto(this Employee employee)
        {
            return new EmployeeDto
            {
                Id = employee.Id,
                UserId = employee.UserId,
                StaffNumber = employee.StaffNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                MiddleName = employee.MiddleName,
                Post = employee.Post,
                Department = employee.Department,
                IsActive = employee.IsActive,
                CreatedAt = employee.CreatedAt,
                CreatedBy = employee.CreatedBy, 
                UpdatedAt = employee.UpdatedAt,
                UpdatedBy = employee.UpdatedBy
            };
        }

        public static EmployeeShortDataDto ToEmployeeShortDataDto(this Employee employee)
        {
            return new EmployeeShortDataDto
            {
                StaffNumber = employee.StaffNumber,
                FullName = $"{employee.LastName} {employee.FirstName} {employee.MiddleName}".Trim(),
                Department = employee.Department,
                Post = employee.Post
            };
        }

        public static Employee ToEntity(this CreateEmployeeDto createDto)
        {
            return new Employee
            {
                Id = Guid.NewGuid(),
                StaffNumber = createDto.StaffNumber,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                MiddleName = createDto.MiddleName,
                Post = createDto.Post,
                Department = createDto.Department,
                IsActive = true
            };
        }

        public static void UpdateEntity(this UpdateEmployeeDto updateDto, Employee entity)
        {
            entity.StaffNumber = updateDto.StaffNumber;
            entity.FirstName = updateDto.FirstName;
            entity.LastName = updateDto.LastName;
            entity.MiddleName = updateDto.MiddleName;
            entity.Post = updateDto.Post; 
            entity.Department = updateDto.Department;
        }

        public static UserListItemDto ToUserListItemDto(this Employee employee, string login, string role)
        {
            return new UserListItemDto
            {
                EmployeeId = employee.Id,
                StaffNumber = employee.StaffNumber,
                FullName = $"{employee.LastName} {employee.FirstName} {employee.MiddleName}".Trim(),
                Post = employee.Post,
                Department = employee.Department,
                Email = employee.User?.Email ?? string.Empty,
                Login = login,
                Role = role,
                IsActive = employee.IsActive
            };
        }
    }
}
