using ClientOpsPortal.Domain.Entities.Common;
using ClientOpsPortal.Domain.Enums;

namespace ClientOpsPortal.Domain.Entities
{
    public class SubscriptionHistoryStep : CreationAuditableEntity
    {
        public Guid IdSubscriptionHistory { get; set; }
        public SubscriptionActionStatus Status { get; set; }
        public string? Message { get; set; }
    }
}
