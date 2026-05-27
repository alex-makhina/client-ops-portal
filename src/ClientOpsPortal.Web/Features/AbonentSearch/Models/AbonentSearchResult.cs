namespace ClientOpsPortal.Web.Features.AbonentSearch.Models
{
    public class AbonentSearchResult
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
