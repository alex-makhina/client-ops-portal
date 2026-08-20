using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs
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

    public class SubscriptionHistoryFullDto : AuditableDto
    {
        public Guid SubscriptionId { get; set; }
        public SubscriptionActionType ActionType { get; set; }
        public SubscriptionActionStatus Status { get; set; }
        public Guid TariffPlanId { get; set; }
        public string TariffPlanName { get; set; }
        public string ServiceName { get; set; }
        public string ContractNumber { get; set; }
        public Guid AbonentId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public List<SubscriptionHistoryStep> Steps { get; set; } = [];
    }

    public class CreateSubscriptionHistoryDto
    {
        public required Guid SubscriptionId { get; set; }
        public required SubscriptionActionType ActionType { get; set; }
        public required SubscriptionActionStatus Status { get; set; }
        public required Guid TariffPlanId { get; set; }
        public string TariffPlanName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        public required Guid AbonentId { get; set; }
        public required DateTimeOffset StartDate { get; set; }
        public List<SubscriptionHistoryStep> Steps { get; set; } = [];
    }

    public class UpdateSubscriptionHistoryDto
    {
        public required SubscriptionActionStatus Status { get; set; }
    }
}
