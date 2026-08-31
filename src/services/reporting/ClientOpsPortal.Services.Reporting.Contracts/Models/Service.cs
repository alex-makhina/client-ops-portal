namespace ClientOpsPortal.Services.Reporting.Contracts.Models
{
    public class Service : BaseEntity
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTimeOffset BeginDate { get; set; } = DateTime.UtcNow;
        public DateTimeOffset? EndDate { get; set; }
        public ICollection<TariffPlan> TariffPlans { get; set; } = [];
    }
}
