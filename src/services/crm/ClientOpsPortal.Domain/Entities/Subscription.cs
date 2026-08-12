using ClientOpsPortal.Domain.Entities.Common;
using ClientOpsPortal.Domain.Interfaces.Entities;

namespace ClientOpsPortal.Domain.Entities
{
    public class Subscription : AuditableEntity, IPeriodEntity
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