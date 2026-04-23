using ClientOpsPortal.Domain.Entities.Common;
using ClientOpsPortal.Domain.Interfaces.Entities;

namespace ClientOpsPortal.Domain.Entities
{
    public class Employee : AuditableEntity
    {
        public required string StaffNumber { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public Guid UserId { get; set; }
        public required string Post { get; set; }
        public string? Department { get; set; }

        public User? User { get; set; }
    }
}
