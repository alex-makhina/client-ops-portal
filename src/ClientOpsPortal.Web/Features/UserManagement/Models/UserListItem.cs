namespace ClientOpsPortal.Web.Features.UserManagement.Models
{
    public class UserListItem
    {
        public Guid EmployeeId { get; set; }
        public string StaffNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Post { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
