using ClientOpsPortal.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Application.DTOs
{  
    public class AbonentDto : AuditableDto
    {
        public required string UserId { get; set; }
        public required string IdentificationNumber { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string? MiddleName { get; set; }
        public required string AccountNumber { get; set; } 
    }

    public class AbonentShortDataDto
    {
        public required string AccountNumber { get; set; }
        public required string FullName { get; set; }
    }

    public class CreateAbonentDto : BaseDto
    {
        public required string UserId { get; set; }
        public required string IdentificationNumber { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string? MiddleName { get; set; }
        public required string AccountNumber { get; set; }
    }

    public class UpdateAbonentDto : BaseDto
    {
        public required string IdentificationNumber { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string? MiddleName { get; set; }
        public required string AccountNumber { get; set; }
    }
}
