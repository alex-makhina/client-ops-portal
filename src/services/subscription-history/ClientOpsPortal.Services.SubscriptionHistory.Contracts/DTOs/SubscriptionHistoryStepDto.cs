using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs
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
