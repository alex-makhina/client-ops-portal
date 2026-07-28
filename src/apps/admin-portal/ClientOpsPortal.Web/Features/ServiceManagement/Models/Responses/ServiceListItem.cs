namespace ClientOpsPortal.Web.Features.ServiceManagement.Models.Responses
{
    public class ServiceListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}
