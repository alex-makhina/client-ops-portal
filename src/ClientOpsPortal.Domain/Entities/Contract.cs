using ClientOpsPortal.Domain.Entities.Common;
using ClientOpsPortal.Domain.Interfaces.Entities;

namespace ClientOpsPortal.Domain.Entities
{
    public class Contract : AuditableEntity, IPeriodEntity
    {
        public required string ContractNumber { get; set; }

        public Guid AbonentId { get; set; }
        public DateTimeOffset BeginDate { get; set; } = DateTime.UtcNow;
        public DateTimeOffset? EndDate { get; set; }

        public Abonent? Abonent { get; set; }
        public List<Subscription> Subscriptions { get; set; } = [];
    }
}
