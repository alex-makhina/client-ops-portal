using ClientOpsPortal.Services.Directory.Contracts.Models;

namespace ClientOpsPortal.Services.Directory.Contracts.Models
{
    public class Service : AuditableEntity
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTimeOffset BeginDate { get; set; } = DateTime.UtcNow;
        public DateTimeOffset? EndDate { get; set; }
        public ICollection<TariffPlan> TariffPlans { get; set; } = [];
    }
}
