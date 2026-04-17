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
                UserId = createDto.UserId,
                StaffNumber = createDto.StaffNumber,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                MiddleName = createDto.MiddleName,
                Post = createDto.Post,
                Department = createDto.Department
            };
        }

        public static void UpdateEntity(this UpdateEmployeeDto updateDto, Employee entity)
        {
            if (!string.IsNullOrWhiteSpace(updateDto.StaffNumber))
                entity.StaffNumber = updateDto.StaffNumber;

            if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
                entity.FirstName = updateDto.FirstName;

            if (!string.IsNullOrWhiteSpace(updateDto.LastName))
                entity.LastName = updateDto.LastName;

            if (updateDto.MiddleName != null)  
                entity.MiddleName = updateDto.MiddleName;

            if (!string.IsNullOrWhiteSpace(updateDto.Post))
                entity.Post = updateDto.Post;

            if (updateDto.Department != null)  
                entity.Department = updateDto.Department;
        }
    }
}
