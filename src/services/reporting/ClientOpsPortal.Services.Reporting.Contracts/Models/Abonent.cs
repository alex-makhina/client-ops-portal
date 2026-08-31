using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.Reporting.Contracts.Models
{
    public class Abonent : BaseEntity
    {
        public required string IdentificationNumber { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
        public Guid UserId { get; set; }
        public required string AccountNumber { get; set; }
    }
}
