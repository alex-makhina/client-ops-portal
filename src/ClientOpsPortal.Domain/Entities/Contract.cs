using ClientOpsPortal.Domain.Entities.Common;
using ClientOpsPortal.Domain.Interfaces.Entities;

namespace ClientOpsPortal.Domain.Entities
{
    public class Contract : AuditableEntity, IPeriodEntity
    {
        public required string ContractNumber { get; set; }

        public Guid IdAbonent { get; set; }
        public DateTime BeginDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        public Abonent? Abonent { get; set; }
        public Service? Service { get; set; }
        public List<Subscription> Subscriptions { get; set; } = new List<Subscription>(capacity: 1);
    }
}
