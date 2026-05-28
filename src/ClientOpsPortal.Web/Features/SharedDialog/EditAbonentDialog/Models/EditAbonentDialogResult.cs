namespace ClientOpsPortal.Web.Features.SharedDialog.EditAbonentDialog.Models
{
    public class EditAbonentDialogResult
    {
        public bool IsEditMode { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string IdentificationNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}