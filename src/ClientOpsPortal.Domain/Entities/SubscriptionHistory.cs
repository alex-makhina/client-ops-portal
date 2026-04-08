using ClientOpsPortal.Domain.Entities.Common;
using ClientOpsPortal.Domain.Enums;

namespace ClientOpsPortal.Domain.Entities
{
    public class SubscriptionHistory : AuditableEntity
    {
        public Guid SubscriptionId { get; set; }
        public SubscriptionActionType ActionType { get; set; }
        public SubscriptionActionStatus Status { get; set; }
        public Guid TariffPlanId { get; set; }
        public DateTimeOffset StartDate { get; set; } = DateTimeOffset.UtcNow;
        public List<SubscriptionHistoryStep> Steps { get; set; } = [];

        public TariffPlan? TariffPlan { get; set; }
        public Subscription? Subscription { get; set; }
    }
}
