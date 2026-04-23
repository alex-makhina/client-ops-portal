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
    }

    public class UpdateEmployeeDto : BaseDto
    {
        public required string StaffNumber { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public required string Post { get; set; }
        public string? Department { get; set; }
    } 
}
