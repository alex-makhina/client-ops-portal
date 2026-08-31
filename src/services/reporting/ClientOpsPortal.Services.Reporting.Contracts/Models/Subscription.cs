namespace ClientOpsPortal.Services.Reporting.Contracts.Models
{
    public class Subscription : BaseEntity
    {
        public Guid ContractId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid TariffPlanId { get; set; }
        public DateTimeOffset BeginDate { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? EndDate { get; set; }

        public Contract? Contract { get; set; }
        public Service? Service { get; set; }
        public TariffPlan? TariffPlan { get; set; }
    }
}
