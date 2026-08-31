namespace ClientOpsPortal.Services.Reporting.Contracts.Models
{
    public class Employee : BaseEntity
    {
        public required string StaffNumber { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public Guid UserId { get; set; }
        public required string Post { get; set; }
        public string? Department { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
