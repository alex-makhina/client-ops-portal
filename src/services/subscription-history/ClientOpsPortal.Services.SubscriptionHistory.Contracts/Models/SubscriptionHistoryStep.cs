using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models
{
    public class SubscriptionHistoryStep : CreationAuditableEntity
    {
        [BsonRepresentation(BsonType.String)]
        public Guid SubscriptionHistoryId { get; set; }
        public SubscriptionActionStatus Status { get; set; }
        public string? Message { get; set; }

        public SubscriptionHistory SubscriptionHistory { get; set; }
    }
}
