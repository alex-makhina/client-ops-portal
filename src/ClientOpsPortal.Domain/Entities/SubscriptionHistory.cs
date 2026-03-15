using ClientOpsPortal.Domain.Entities.Common;
using ClientOpsPortal.Domain.Enums;

namespace ClientOpsPortal.Domain.Entities
{
    public class SubscriptionHistory : AuditableEntity
    {
        public Guid IdSubscription { get; set; }
        public SubscriptionActionType ActionType { get; set; }
        public SubscriptionActionStatus Status { get; set; }
        public Guid IdTariffPlan { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public List<SubscriptionHistoryStep> Steps { get; set; } = new List<SubscriptionHistoryStep>();

        public TariffPlan? TariffPlan { get; set; }
        public Subscription? Subscription { get; set; }
    }
}
