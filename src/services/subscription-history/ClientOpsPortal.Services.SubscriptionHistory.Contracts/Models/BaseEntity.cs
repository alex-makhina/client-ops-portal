using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models
{
    public abstract class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
    }

    public abstract class CreationAuditableEntity : BaseEntity
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public abstract class AuditableEntity : CreationAuditableEntity
    {
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
