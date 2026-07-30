using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.Reporting.Contracts.Models
{
    public class Contract : AuditableEntity
    {
        public required string ContractNumber { get; set; }

        public Guid AbonentId { get; set; }
        public DateTimeOffset BeginDate { get; set; } = DateTime.UtcNow;
        public DateTimeOffset? EndDate { get; set; }

        public Abonent? Abonent { get; set; }
        public List<Subscription> Subscriptions { get; set; } = [];
    }
}
