namespace ClientOpsPortal.Web.Features.ClientCard.Models
{
    public enum SubscriptionActionType
    {
        Open = 1,
        Close = 2,
        TariffChange = 3,
    }

    public enum SubscriptionActionStatus
    {
        InProgress = 0,
        Completed = 1,
        Failed = 2,
        Cancelled = 3,
        Pending = 4,
    }

    public class SubscriptionHistoryDto
    {
        public Guid Id { get; set; }
        public Guid SubscriptionId { get; set; }
        public int ActionType { get; set; }
        public int Status { get; set; }
        public Guid TariffPlanId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<SubscriptionHistoryStepDto> Steps { get; set; } = new();
        public string? ServiceName { get; set; }
        public string? TariffPlanName { get; set; }
        public string? ContractNumber { get; set; }
    }

    public class SubscriptionHistoryStepDto
    {
        public Guid Id { get; set; }
        public Guid SubscriptionHistoryId { get; set; }
        public int Status { get; set; }
        public string? Message { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class SubscriptionHistoryStepItem
    {
        public Guid Id { get; set; }
        public Guid SubscriptionHistoryId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class SubscriptionHistoryItem
    {
        public Guid Id { get; set; }
        public Guid SubscriptionId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? ServiceName { get; set; }
        public string? TariffPlanName { get; set; }
        public string? ContractNum { get; set; }
        public int StepsCount { get; set; }
        public List<SubscriptionHistoryStepItem> Steps { get; set; } = new();
    }
}