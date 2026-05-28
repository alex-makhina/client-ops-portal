namespace ClientOpsPortal.Web.Features.ClientCard.Models
{
    public class AbonentDetail
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string IdentificationNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }
}
