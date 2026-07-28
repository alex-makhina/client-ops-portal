namespace ClientOpsPortal.Web.Features.UserManagement.Models
{
    public class CreateUserRequest
    {
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
