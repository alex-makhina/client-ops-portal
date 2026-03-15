using ClientOpsPortal.Domain.Entities.Common;
using ClientOpsPortal.Domain.Interfaces.Entities;

namespace ClientOpsPortal.Domain.Entities
{
    public class Subscription : AuditableEntity, IPeriodEntity
    {
        public Guid IdContract { get; set; }
        public Guid IdTariffPlan { get; set; }
        public DateTime BeginDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        public Contract? Contract { get; set; }
        public TariffPlan? TariffPlan { get; set; }
    }
}
