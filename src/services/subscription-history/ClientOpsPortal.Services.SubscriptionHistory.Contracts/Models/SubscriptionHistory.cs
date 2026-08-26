using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models
{
    public class SubscriptionHistory : AuditableEntity
    {
        [BsonRepresentation(BsonType.String)]
        public Guid SubscriptionId { get; set; }
        public SubscriptionActionType ActionType { get; set; }
        public SubscriptionActionStatus Status { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid TariffPlanId { get; set; }
        public string TariffPlanName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        [BsonRepresentation(BsonType.String)]
        public Guid AbonentId { get; set; }
        public DateTimeOffset StartDate { get; set; } = DateTimeOffset.UtcNow;
        public List<SubscriptionHistoryStep> Steps { get; set; } = [];
    }
}
