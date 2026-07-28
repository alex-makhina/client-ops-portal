namespace ClientOpsPortal.Web.Features.UserManagement
{
    public class UserEditDialogResult
    {
        public bool IsEditMode { get; set; }
        public Guid EmployeeId { get; set; }
        public string StaffNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string Post { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
