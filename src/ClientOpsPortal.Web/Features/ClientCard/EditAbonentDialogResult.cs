namespace ClientOpsPortal.Web.Features.ClientCard
{
    public class EditAbonentDialogResult
    {
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string IdentificationNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
    }
}
