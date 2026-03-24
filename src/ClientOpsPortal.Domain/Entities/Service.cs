using ClientOpsPortal.Domain.Entities.Common;
using ClientOpsPortal.Domain.Interfaces.Entities;

namespace ClientOpsPortal.Domain.Entities
{
    public class Service : AuditableEntity, IPeriodEntity
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTime BeginDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public ICollection<TariffPlan> TariffPlans { get; set; } = [];
    }
}
