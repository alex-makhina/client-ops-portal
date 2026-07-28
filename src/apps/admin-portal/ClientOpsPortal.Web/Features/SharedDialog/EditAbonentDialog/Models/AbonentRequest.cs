namespace ClientOpsPortal.Web.Features.SharedDialog.EditAbonentDialog.Models
{
    public class AbonentRequest
    {
        public string IdentificationNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
