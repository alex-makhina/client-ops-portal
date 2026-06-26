using ClientOpsPortal.Application.DTOs.Common;

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
        public string ContractNumber { get; set; }
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
}
