namespace ClientOpsPortal.Services.Reporting.Contracts.Models
{
    public class TariffPlan : BaseEntity
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public Guid ServiceId { get; set; }
        public DateTimeOffset BeginDate { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? EndDate { get; set; }

        public Service? Service { get; set; }
    }
}
