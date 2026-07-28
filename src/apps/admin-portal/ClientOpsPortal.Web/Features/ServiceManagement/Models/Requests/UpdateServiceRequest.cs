namespace ClientOpsPortal.Web.Features.ServiceManagement.Models.Requests
{
    public class UpdateServiceRequest
    {
        public bool IsEditMode { get; set; }
        public Guid ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public List<TariffPlanRequest> TariffPlans { get; set; } = new();
    }
}