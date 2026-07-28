using ClientOpsPortal.Application.DTOs.Common;
using ClientOpsPortal.Domain.Enums;

namespace ClientOpsPortal.Application.DTOs
{
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

    public class UpdateSubscriptionHistoryStepDto
    {
        public SubscriptionActionStatus? Status { get; set; }
        public string? Message { get; set; }
    }
}