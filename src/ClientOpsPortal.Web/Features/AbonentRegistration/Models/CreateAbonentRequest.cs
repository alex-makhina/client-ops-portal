namespace ClientOpsPortal.Web.Features.AbonentRegistration.Models
{
    public class CreateAbonentRequest
    {
        public string IdentificationNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
