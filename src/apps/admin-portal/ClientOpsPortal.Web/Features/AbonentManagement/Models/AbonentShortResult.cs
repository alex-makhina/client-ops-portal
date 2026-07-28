namespace ClientOpsPortal.Web.Features.AbonentManagement.Models
{
    public class AbonentShortResult
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
