using ClientOpsPortal.Application.DTOs.Common;
using ClientOpsPortal.Domain.Enums;

namespace ClientOpsPortal.Application.DTOs
{
    public class SubscriptionDto : AuditableDto
    {
        public Guid ContractId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid TariffPlanId { get; set; }
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }

    public class SubscriptionFullDataDto 
    {
        public Guid Id { get; set; }
        public Guid ContractId { get; set; }
        public required string ContractNumber { get; set; }
        public Guid ServiceId { get; set; }
        public required string ServiceName { get; set; }
        public Guid TariffPlanId { get; set; }
        public required string TariffPlanName { get; set; }
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public bool IsActive => EndDate == null || EndDate > DateTimeOffset.UtcNow;
    }

    public class UpdateSubscriptionDto : BaseDto
    {
        public Guid? TariffPlanId { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }

    public class SubscriptionHistoryEventDto
    {
        public Guid SubscriptionId { get; set; }
        public SubscriptionActionType ActionType { get; set; }
        public SubscriptionActionStatus Status { get; set; }
        public Guid TariffPlanId { get; set; }
        public string TariffPlanName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        public Guid AbonentId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
