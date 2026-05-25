using ClientOpsPortal.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Application.DTOs
{  
    public class EmployeeDto : AuditableDto
    {
        public Guid UserId { get; set; }
        public required string StaffNumber { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public required string Post { get; set; } 
        public string? Department { get; set; }
        public string? Login { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class EmployeeShortDataDto 
    {
        public required string StaffNumber { get; set; }
        public required string FullName { get; set; }
        public required string Post { get; set; }
        public string? Department { get; set; }
    }

    public class CreateEmployeeDto : BaseDto
    {
        public required string StaffNumber { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public required string Post { get; set; }
        public string? Department { get; set; }
        public required string Email { get; set; }
        public required string Login { get; set; }
        public string? Password { get; set; }
        public required string Role { get; set; }
    }

    public class UpdateEmployeeDto : BaseDto
    {
        public required string StaffNumber { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public required string Post { get; set; }
        public string? Department { get; set; }
        public required string Role { get; set; }
    }

    public class UserListItemDto
    {
        public Guid EmployeeId { get; set; }
        public required string StaffNumber { get; set; }
        public required string FullName { get; set; }
        public required string Post { get; set; }
        public string? Department { get; set; }
        public required string Email { get; set; }
        public required string Login { get; set; }
        public required string Role { get; set; }
        public bool IsActive { get; set; }
    }

    public class ToggleUserStatusDto
    {
        public bool IsActive { get; set; }
    }
}
