using ClientOpsPortal.Application.DTOs.Common;
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
        public List<SubscriptionHistoryStepDto> Steps { get; set; } = [];
    }

    public class SubscriptionHistoryFullDto : AuditableDto
    {
        public Guid SubscriptionId { get; set; }
        public SubscriptionActionType ActionType { get; set; }
        public SubscriptionActionStatus Status { get; set; }
        public Guid TariffPlanId { get; set; }
        public string TariffPlanName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        public DateTimeOffset StartDate { get; set; }
        public List<SubscriptionHistoryStepDto> Steps { get; set; } = [];
    }

    public class SubscriptionHistoryStepDto : CreationAuditableDto
    {
        public Guid SubscriptionHistoryId { get; set; }
        public SubscriptionActionStatus Status { get; set; }
        public string? Message { get; set; }
    }

    public class CreateSubscriptionHistoryStepDto
    {
        public required Guid SubscriptionHistoryId { get; set; }
        public required SubscriptionActionStatus Status { get; set; }
        public string? Message { get; set; }
    }
}