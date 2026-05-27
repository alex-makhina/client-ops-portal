namespace ClientOpsPortal.Web.Features.ServiceManagement.Models
{
    public class CreateTariffPlanFromServiceRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public Guid ServiceId { get; set; }
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}
