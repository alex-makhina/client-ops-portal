using ClientOpsPortal.Application.DTOs.Common;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Enums;

namespace ClientOpsPortal.Application.DTOs
{
    public class SubscriptionHistoryDto : AuditableDto
    {
        public Guid SubscriptionId { get; set; }
        public SubscriptionActionType ActionType { get; set; }
        public SubscriptionActionStatus Status { get; set; }
        public Guid TariffPlanId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public List<SubscriptionHistoryStep> Steps { get; set; } = [];
    }

    public class CreateSubscriptionHistoryDto 
    {
        public required Guid SubscriptionId { get; set; }
        public required SubscriptionActionType ActionType { get; set; }
        public required SubscriptionActionStatus Status { get; set; }
        public required Guid TariffPlanId { get; set; }
        public required DateTimeOffset StartDate { get; set; }
        public List<SubscriptionHistoryStep> Steps { get; set; } = [];
    }

    public class UpdateSubscriptionHistoryDto 
    {
        public required SubscriptionActionStatus Status { get; set; }
    }

}
